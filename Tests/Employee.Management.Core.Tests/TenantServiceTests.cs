using AutoMapper;
using Employee.Management.Core.BusinessContext;
using Employee.Management.Core.Exceptions;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos.RequestDtos;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Employee.Management.Core.Tests;

// Typed exceptions matter: the middleware maps them to 400/404 with the message,
// which the admin console shows to the user. A plain Exception becomes a generic 500.
public class TenantServiceTests
{
    private readonly Mock<ITenantRepository> _tenants = new();
    private readonly TenantService _sut;

    public TenantServiceTests()
    {
        _sut = new TenantService(_tenants.Object, Mock.Of<IMapper>(), NullLogger<TenantService>.Instance);
    }

    [Fact]
    public async Task CreateTenant_ThrowsBadRequest_WhenNameExists()
    {
        _tenants.Setup(t => t.CheckForExistingName("Acme", null)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<BadRequestException>(() => _sut.CreateTenant(new CreateTenantDto { Name = "Acme" }));
        Assert.Equal("Tenant name already exists.", ex.Message);
    }

    [Fact]
    public async Task CreateTenant_ReturnsTrue_WhenSaved()
    {
        _tenants.Setup(t => t.CheckForExistingName("Acme", null)).ReturnsAsync(false);
        _tenants.Setup(t => t.CreateTenantAsync(It.Is<Tenant>(x => x.Name == "Acme" && x.Logo == "acme.png"))).ReturnsAsync(1);

        Assert.True(await _sut.CreateTenant(new CreateTenantDto { Name = "Acme", Logo = "acme.png" }));
    }

    [Fact]
    public async Task EditTenant_ThrowsNotFound_WhenTenantMissing()
    {
        _tenants.Setup(t => t.GetTenantInfoAsync(9)).ReturnsAsync((Tenant)null!);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.EditTenant(9, new EditTenantDto { Name = "Acme" }));
    }

    [Fact]
    public async Task EditTenant_ThrowsBadRequest_WhenNameTakenByAnotherTenant()
    {
        _tenants.Setup(t => t.GetTenantInfoAsync(1)).ReturnsAsync(new Tenant { TenantId = 1, Name = "Acme" });
        _tenants.Setup(t => t.CheckForExistingName("Globex", 1)).ReturnsAsync(true);

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.EditTenant(1, new EditTenantDto { Name = "Globex" }));
    }

    [Fact]
    public async Task DeleteTenant_ThrowsNotFound_WhenTenantMissing()
    {
        _tenants.Setup(t => t.GetTenantInfoAsync(9)).ReturnsAsync((Tenant)null!);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteTenant(9));
    }
}
