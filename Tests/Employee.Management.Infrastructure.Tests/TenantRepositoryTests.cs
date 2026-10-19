using AutoMapper;
using Employee.Management.Api.Mapping;
using Employee.Management.Infrastructure.Repositories;
using Employee.Management.Models.DatabaseModels;
using Microsoft.Extensions.Logging.Abstractions;

namespace Employee.Management.Infrastructure.Tests;

public class TenantRepositoryTests
{
    private readonly EmployeeManagementDbContext _context = TestDb.NewContext();

    // The real profile, so the test covers the OrganizationCount mapping that ProjectTo uses.
    private static readonly IMapper Mapper =
        new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private TenantRepository Repo() => new(_context, Mapper);

    private void AddTenant(int tenantId, string name, int organizationCount)
    {
        _context.Tenants.Add(new Tenant { TenantId = tenantId, Uid = Guid.NewGuid(), Name = name, Logo = $"{name}.png" });
        for (var i = 0; i < organizationCount; i++)
            _context.Organizations.Add(new Organization { Uid = Guid.NewGuid(), TenantId = tenantId, Name = $"{name} Org {i}" });
    }

    [Fact]
    public async Task GetAllTenantsAsync_ReturnsOrganizationCountPerTenant()
    {
        AddTenant(1, "Acme", organizationCount: 3);
        AddTenant(2, "Globex", organizationCount: 1);
        await _context.SaveChangesAsync();

        var tenants = await Repo().GetAllTenantsAsync();

        Assert.Equal(3, tenants.Single(t => t.TenantId == 1).OrganizationCount);
        Assert.Equal(1, tenants.Single(t => t.TenantId == 2).OrganizationCount);
    }

    [Fact]
    public async Task GetAllTenantsAsync_ReturnsZero_ForTenantWithNoOrganizations()
    {
        AddTenant(1, "Empty", organizationCount: 0);
        await _context.SaveChangesAsync();

        var tenant = Assert.Single(await Repo().GetAllTenantsAsync());

        Assert.Equal(0, tenant.OrganizationCount);
    }

    [Fact]
    public async Task GetAllTenantsAsync_MapsTenantFields()
    {
        var uid = Guid.NewGuid();
        _context.Tenants.Add(new Tenant { TenantId = 7, Uid = uid, Name = "Acme", Logo = "acme.png" });
        await _context.SaveChangesAsync();

        var tenant = Assert.Single(await Repo().GetAllTenantsAsync());

        Assert.Equal(7, tenant.TenantId);
        Assert.Equal(uid, tenant.Uid);
        Assert.Equal("Acme", tenant.Name);
        Assert.Equal("acme.png", tenant.Logo);
    }
}
