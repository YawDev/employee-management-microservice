using AutoMapper;
using Employee.Management.Core.BusinessContext;
using Employee.Management.Core.Exceptions;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos.RequestDtos;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Employee.Management.Core.Tests;

public class CompanyServiceTests
{
    private readonly Mock<IOrganizationRepository> _organizations = new();
    private readonly Mock<IDepartmentRepository> _departments = new();
    private readonly Mock<ITenantRepository> _tenants = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CompanyService _sut;

    public CompanyServiceTests()
    {
        _sut = new CompanyService(
            _organizations.Object, _departments.Object, _tenants.Object,
            _mapper.Object, NullLogger<CompanyService>.Instance);
    }

    [Fact]
    public async Task CreateDepartment_Throws_WhenOrganizationNotFound()
    {
        _organizations.Setup(o => o.GetOrganizationInfoAsync(It.IsAny<int>())).ReturnsAsync((Organization?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateDepartment(new SaveDepartmentDto { OrganizationId = 1, Name = "Engineering" }));
    }

    [Fact]
    public async Task CreateDepartment_Throws_WhenNameAlreadyExistsInOrganization()
    {
        _organizations.Setup(o => o.GetOrganizationInfoAsync(It.IsAny<int>())).ReturnsAsync(new Organization());
        _departments.Setup(d => d.CheckForExistingName("Engineering", 1, It.IsAny<Guid?>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<BadRequestException>(
            () => _sut.CreateDepartment(new SaveDepartmentDto { OrganizationId = 1, Name = "Engineering" }));
    }

    [Fact]
    public async Task CreateDepartment_ReturnsTrue_WhenValid()
    {
        _organizations.Setup(o => o.GetOrganizationInfoAsync(It.IsAny<int>())).ReturnsAsync(new Organization());
        _departments.Setup(d => d.CheckForExistingName(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid?>())).ReturnsAsync(false);
        _departments.Setup(d => d.CreateDepartmentAsync(It.IsAny<Department>())).ReturnsAsync(1);

        var result = await _sut.CreateDepartment(new SaveDepartmentDto { OrganizationId = 1, Name = "Engineering" });

        Assert.True(result);
    }
}
