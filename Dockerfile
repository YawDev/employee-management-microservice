# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only csproj files first so the restore layer caches across source-only changes
COPY Employee.Management.Api/Employee.Management.Api.csproj                       Employee.Management.Api/
COPY Employee.Management.Core/Employee.Management.Core.csproj                     Employee.Management.Core/
COPY Employee.Management.Infrastructure/Employee.Management.Infrastructure.csproj Employee.Management.Infrastructure/
COPY Employee.Management.Models/Employee.Management.Models.csproj                 Employee.Management.Models/
RUN dotnet restore Employee.Management.Api/Employee.Management.Api.csproj

COPY . .
RUN dotnet publish Employee.Management.Api/Employee.Management.Api.csproj \
      -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Employee.Management.Api.dll"]
