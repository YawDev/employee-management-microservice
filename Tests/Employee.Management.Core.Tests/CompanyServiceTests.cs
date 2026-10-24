using AutoMapper;
using Employee.Management.Api.Mapping;
using Employee.Management.Core.BusinessContext;
using Employee.Management.Core.Exceptions;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos.RequestDtos;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

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

    [Fact]
    public async Task GetDepartmentInfo_Throws_WhenDepartmentNotFound()
    {
        _departments.Setup(d => d.GetDepartmentInfoAsync(It.IsAny<Guid>())).ReturnsAsync((Department?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetDepartmentInfo(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetDepartmentInfo_ReturnsMembersWithIdsNamesAndJobTitles()
    {
        var departmentId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var alanId = Guid.NewGuid();
        var graceId = Guid.NewGuid();
        _departments.Setup(d => d.GetDepartmentInfoAsync(departmentId)).ReturnsAsync(new Department
        {
            DepartmentId = departmentId,
            OrganizationId = 1,
            Name = "Engineering",
            Description = "Builds things",
            Managers =
            {
                // Distinct people: a manager's own Employee row would be filtered out of Employees.
                new Manager { ManagerId = managerId, DomainUserId = Guid.NewGuid(), DomainUser = new DomainUser { FirstName = "Ada", LastName = "Lovelace" } },
            },
            Employees =
            {
                new EmployeeEntity { EmployeeId = alanId, DomainUserId = Guid.NewGuid(), JobTitle = "Engineer", DomainUser = new DomainUser { FirstName = "Alan", LastName = "Turing" } },
                new EmployeeEntity { EmployeeId = graceId, DomainUserId = Guid.NewGuid(), JobTitle = null, DomainUser = new DomainUser { FirstName = "Grace", LastName = "Hopper" } },
            },
        });
        // Real profile, so the test covers the DomainUser -> member mapping too.
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>(), NullLoggerFactory.Instance).CreateMapper();
        var sut = new CompanyService(
            _organizations.Object, _departments.Object, _tenants.Object,
            mapper, NullLogger<CompanyService>.Instance);

        var result = await sut.GetDepartmentInfo(departmentId);

        Assert.Equal(departmentId, result.DepartmentId);
        Assert.Equal(1, result.OrganizationId);
        Assert.Equal("Engineering", result.Name);
        Assert.Equal("Builds things", result.Description);

        var manager = Assert.Single(result.Managers);
        Assert.Equal(managerId, manager.ManagerId);
        Assert.Equal("Ada", manager.FirstName);
        Assert.Equal("Lovelace", manager.LastName);

        Assert.Collection(result.Employees,
            e => { Assert.Equal(alanId, e.EmployeeId); Assert.Equal("Alan", e.FirstName); Assert.Equal("Turing", e.LastName); Assert.Equal("Engineer", e.JobTitle); },
            e => { Assert.Equal(graceId, e.EmployeeId); Assert.Equal("Grace", e.FirstName); Assert.Equal("Hopper", e.LastName); Assert.Null(e.JobTitle); });
    }

    [Fact]
    public async Task GetDepartmentInfo_ListsAManagerOnlyUnderManagers()
    {
        var departmentId = Guid.NewGuid();
        var adaUserId = Guid.NewGuid();
        var adaAsManager = new Manager { ManagerId = Guid.NewGuid(), DomainUserId = adaUserId, DomainUser = new DomainUser { DomainUserId = adaUserId, FirstName = "Ada", LastName = "Lovelace" } };
        _departments.Setup(d => d.GetDepartmentInfoAsync(departmentId)).ReturnsAsync(new Department
        {
            DepartmentId = departmentId,
            Name = "Engineering",
            Managers = { adaAsManager },
            Employees =
            {
                new EmployeeEntity { EmployeeId = Guid.NewGuid(), DomainUserId = adaUserId, DomainUser = adaAsManager.DomainUser },
                new EmployeeEntity { EmployeeId = Guid.NewGuid(), DomainUserId = Guid.NewGuid(), DomainUser = new DomainUser { FirstName = "Alan", LastName = "Turing" } },
            },
        });
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>(), NullLoggerFactory.Instance).CreateMapper();
        var sut = new CompanyService(_organizations.Object, _departments.Object, _tenants.Object, mapper, NullLogger<CompanyService>.Instance);

        var result = await sut.GetDepartmentInfo(departmentId);

        Assert.Equal("Ada", Assert.Single(result.Managers).FirstName);
        Assert.Equal("Alan", Assert.Single(result.Employees).FirstName);
    }
}
