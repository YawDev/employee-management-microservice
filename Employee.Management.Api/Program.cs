using Employee.Management.Api.Logging;
using Employee.Management.Api.Mapping;
using Employee.Management.Api.Middleware;
using Employee.Management.Core.BusinessContext;
using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Infrastructure;
using Employee.Management.Infrastructure.Repositories;
using Employee.Management.Models.DatabaseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Console logging: a compact "[CorrelationId] path message" line in Development, structured JSON
// once hosted (so aggregators — CloudWatch, journald/Loki, Azure Log Analytics — can query the
// CorrelationId field). Both rely on the per-request correlation scope.
builder.Logging.ClearProviders();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole(o => o.FormatterName = CompactConsoleFormatter.FormatterName);
    builder.Logging.AddConsoleFormatter<CompactConsoleFormatter, ConsoleFormatterOptions>();

    // Dev-only: surface our own Debug logs and EF Core SQL query logs.
    builder.Logging.AddFilter("Employee.Management", LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}
else
{
    builder.Logging.AddJsonConsole(o => o.IncludeScopes = true);
    // Hosted: keep EF query noise out of the logs.
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
}

// Add services to the container.
#region Register services
builder.Services.AddOpenApi();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EmployeeManagementDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://microsoft.com
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee.Management.Api",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token only. Example: eyJhbGciOi..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// Configure ASP.NET Core Identity with Guid keys
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<EmployeeManagementDbContext>()
.AddDefaultTokenProviders();

// Liveness/readiness probe for the deployment pipeline and uptime monitoring. The DbContext check
// opens a real connection, so a deploy fails fast if the app can't reach the database.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EmployeeManagementDbContext>("database");

// Validate the signing key at startup rather than on the first request. The null case is caught
// below, but an empty or too-short key slips through and throws "key length is zero" per-request —
// turning every call, including /health, into a 500. HMAC-SHA256 needs at least 256 bits.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || System.Text.Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 bytes. Set the Jwt__Key environment variable " +
        "(generate one with: openssl rand -base64 64). It must match the identity service exactly, " +
        "or every token this service receives will fail validation.");
}

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

var domainList = builder.Configuration
    .GetSection("CorsOriginSettings:DomainList")
    .Get<string[]>()?
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim())
    .ToArray() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(domainList)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configure Authorization policies to enforce role-based access control for different user roles in the application.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("SystemAdmin", p => p.RequireRole("sys-admin"))
    .AddPolicy("CompanyPermission", p => p.RequireRole("sys-admin", "company-admin"))
    // Configure Authorization policies to enforce role-based access control for different user roles in the application.
    .AddPolicy("ManagerPermission", p => p.RequireRole("sys-admin", "manager"))
    .AddPolicy("EmployeePermission", p => p.RequireRole("sys-admin", "employee"));

// Register application services for dependency injection
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IEmployeeManagerService, EmployeeManagerService>();

// Register Repositories for dependency injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IManagerRepository, ManagerRepository>();


// Register AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MapperProfile>();
});
#endregion

#region Configure the HTTP request pipeline
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");
app.UseMiddleware<CorrelationIdMiddleware>(); // Correlation id per request + a request-completion log line
app.UseMiddleware<ExceptionHandlingMiddleware>(); // Centralized Exception handling

// Caddy terminates TLS and forwards plain HTTP to the container. Without this the app only ever
// sees http:// and UseHttpsRedirection below redirects forever. Caddy sets X-Forwarded-Proto:
// https, which makes the redirect correctly no-op. KnownProxies defaults to loopback only — right,
// since Caddy runs on the same host.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection(); // Redirects HTTP requests to HTTPS

app.MapHealthChecks("/health").AllowAnonymous(); // Before auth — the probe carries no token

// Liveness for the Kubernetes probes: runs no checks, so it answers as long as the app is serving.
// Probes must not hit /health — its database check would keep Neon from ever suspending, and a
// Neon cold start would read as a dead pod and get it restarted.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); // Maps controller routes for controller-based APIs
app.Run();
#endregion