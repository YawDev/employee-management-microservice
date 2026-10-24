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

// Service = business rules. Repositories are mocked with Moq, so each test just
// says "when the repo returns X, the service should do Y" — no database involved.
public class EmployeeManagerServiceTests
{
    private readonly Mock<IEmployeeRepository> _employees = new();
    private readonly Mock<IManagerRepository> _managers = new();
    private readonly Mock<IDepartmentRepository> _departments = new();
    private readonly Mock<IOrganizationRepository> _organizations = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly EmployeeManagerService _sut;

    public EmployeeManagerServiceTests()
    {
        _sut = new EmployeeManagerService(
            _employees.Object, _managers.Object, _departments.Object,
            _organizations.Object, _users.Object, _mapper.Object,
            NullLogger<EmployeeManagerService>.Instance);
    }

    // ---- CreateEmployee ----

    [Fact]
    public async Task CreateEmployee_Throws_WhenDomainUserDoesNotExist()
    {
        _users.Setup(u => u.DomainUserExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateEmployee(new SaveEmployeeDto { DomainUserId = Guid.NewGuid(), DepartmentId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task CreateEmployee_Throws_WhenUserAlreadyHasAnEmployeeRecord()
    {
        _users.Setup(u => u.DomainUserExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        _departments.Setup(d => d.GetDepartmentInfoAsync(It.IsAny<Guid>())).ReturnsAsync(new Department());
        _employees.Setup(e => e.CheckForExistingDomainUser(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<BadRequestException>(
            () => _sut.CreateEmployee(new SaveEmployeeDto { DomainUserId = Guid.NewGuid(), DepartmentId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task CreateEmployee_ReturnsTrue_AndSaves_WhenValid()
    {
        _users.Setup(u => u.DomainUserExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        _departments.Setup(d => d.GetDepartmentInfoAsync(It.IsAny<Guid>())).ReturnsAsync(new Department());
        _employees.Setup(e => e.CheckForExistingDomainUser(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(false);
        _employees.Setup(e => e.CreateEmployeeAsync(It.IsAny<EmployeeEntity>())).ReturnsAsync(1);

        var domainUserId = Guid.NewGuid();

        var result = await _sut.CreateEmployee(new SaveEmployeeDto { DomainUserId = domainUserId, DepartmentId = Guid.NewGuid() });

        Assert.True(result);
        _employees.Verify(e => e.CreateEmployeeAsync(It.IsAny<EmployeeEntity>()), Times.Once);
        _users.Verify(u => u.UpdateRoleAsync(domainUserId, "employee"), Times.Once);
    }

    [Fact]
    public async Task CreateEmployee_LeavesRoleAlone_WhenNothingWasSaved()
    {
        _users.Setup(u => u.DomainUserExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        _departments.Setup(d => d.GetDepartmentInfoAsync(It.IsAny<Guid>())).ReturnsAsync(new Department());
        _employees.Setup(e => e.CheckForExistingDomainUser(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(false);
        _employees.Setup(e => e.CreateEmployeeAsync(It.IsAny<EmployeeEntity>())).ReturnsAsync(0);

        var result = await _sut.CreateEmployee(new SaveEmployeeDto { DomainUserId = Guid.NewGuid(), DepartmentId = Guid.NewGuid() });

        Assert.False(result);
        _users.Verify(u => u.UpdateRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    // ---- CreateManager (a manager is an existing employee) ----

    [Fact]
    public async Task CreateManager_Throws_WhenEmployeeNotFound()
    {
        _employees.Setup(e => e.GetEmployeeInfoAsync(It.IsAny<Guid>())).ReturnsAsync((EmployeeEntity?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateManager(new SaveManagerDto { EmployeeId = Guid.NewGuid() }));
        _managers.Verify(m => m.CreateManagerAsync(It.IsAny<Manager>()), Times.Never);
    }

    [Fact]
    public async Task CreateManager_Throws_AndLeavesRoleAlone_WhenEmployeeIsAlreadyAManager()
    {
        _employees.Setup(e => e.GetEmployeeInfoAsync(It.IsAny<Guid>()))
                  .ReturnsAsync(new EmployeeEntity { EmployeeId = Guid.NewGuid(), DomainUserId = Guid.NewGuid() });
        _managers.Setup(m => m.CheckForExistingDomainUser(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<BadRequestException>(
            () => _sut.CreateManager(new SaveManagerDto { EmployeeId = Guid.NewGuid() }));
        _users.Verify(u => u.UpdateRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateManager_CopiesPersonAndDepartmentFromEmployee_AndSetsRole()
    {
        var employee = new EmployeeEntity
        {
            EmployeeId = Guid.NewGuid(),
            DomainUserId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
        };
        _employees.Setup(e => e.GetEmployeeInfoAsync(employee.EmployeeId)).ReturnsAsync(employee);
        _managers.Setup(m => m.CheckForExistingDomainUser(It.IsAny<Guid>(), It.IsAny<Guid?>())).ReturnsAsync(false);
        Manager? saved = null;
        _managers.Setup(m => m.CreateManagerAsync(It.IsAny<Manager>()))
                 .Callback<Manager>(m => saved = m)
                 .ReturnsAsync(1);

        var result = await _sut.CreateManager(new SaveManagerDto { EmployeeId = employee.EmployeeId });

        Assert.True(result);
        Assert.NotNull(saved);
        Assert.Equal(employee.EmployeeId, saved!.EmployeeId);
        Assert.Equal(employee.DomainUserId, saved.DomainUserId);
        Assert.Equal(employee.DepartmentId, saved.DepartmentId);
        _users.Verify(u => u.UpdateRoleAsync(employee.DomainUserId, "manager"), Times.Once);
    }

    [Fact]
    public async Task CreateManagerForOrganization_Throws_WhenEmployeeIsInAnotherOrganization()
    {
        _organizations.Setup(o => o.GetOrganizationInfoAsync(1)).ReturnsAsync(new Organization());
        _employees.Setup(e => e.GetEmployeeInfoForOrganizationAsync(1, It.IsAny<Guid>())).ReturnsAsync((EmployeeEntity?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateManagerForOrganization(new SaveManagerDto { EmployeeId = Guid.NewGuid() }, 1));
        _managers.Verify(m => m.CreateManagerAsync(It.IsAny<Manager>()), Times.Never);
    }

    // ---- DeleteManager ----

    [Fact]
    public async Task DeleteManager_SetsRoleBackToEmployee()
    {
        var managerId = Guid.NewGuid();
        var domainUserId = Guid.NewGuid();
        _managers.Setup(m => m.GetManagerInfoAsync(managerId)).ReturnsAsync(new Manager { ManagerId = managerId, DomainUserId = domainUserId });
        _managers.Setup(m => m.HasReportsAsync(managerId)).ReturnsAsync(false);
        _managers.Setup(m => m.DeleteManagerAsync(managerId)).ReturnsAsync(1);

        Assert.True(await _sut.DeleteManager(managerId));
        _users.Verify(u => u.UpdateRoleAsync(domainUserId, "employee"), Times.Once);
    }

    [Fact]
    public async Task DeleteManager_Throws_AndLeavesRoleAlone_WhenManagerHasReports()
    {
        var managerId = Guid.NewGuid();
        _managers.Setup(m => m.GetManagerInfoAsync(managerId)).ReturnsAsync(new Manager { ManagerId = managerId });
        _managers.Setup(m => m.HasReportsAsync(managerId)).ReturnsAsync(true);

        await Assert.ThrowsAsync<BadRequestException>(() => _sut.DeleteManager(managerId));
        _users.Verify(u => u.UpdateRoleAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    // ---- AddReportToManager (this is where the "already reports to someone" guard lives) ----

    [Fact]
    public async Task AddReportToManager_Throws_WhenManagerNotFound()
    {
        _managers.Setup(m => m.GetManagerInfoAsync(It.IsAny<Guid>())).ReturnsAsync((Manager?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.AddReportToManager(Guid.NewGuid(), new AddReportToManagerDto { ReportIds = new List<Guid> { Guid.NewGuid() } }));
    }

    [Fact]
    public async Task AddReportToManager_Throws_WhenAReportDomainUserDoesNotExist()
    {
        _managers.Setup(m => m.GetManagerInfoAsync(It.IsAny<Guid>())).ReturnsAsync(new Manager());
        // No matching domain users come back → the id is unknown.
        _users.Setup(u => u.AllDomainUsersExistAsync(It.IsAny<List<Guid>>())).ReturnsAsync(new List<DomainUser>());

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.AddReportToManager(Guid.NewGuid(), new AddReportToManagerDto { ReportIds = new List<Guid> { Guid.NewGuid() } }));
    }

    [Fact]
    public async Task AddReportToManager_Throws_AndNeverWrites_WhenReportAlreadyReportsToAManager()
    {
        var alreadyAssigned = Guid.NewGuid();
        _managers.Setup(m => m.GetManagerInfoAsync(It.IsAny<Guid>())).ReturnsAsync(new Manager());
        // The report exists as a person...
        _users.Setup(u => u.AllDomainUsersExistAsync(It.IsAny<List<Guid>>()))
              .ReturnsAsync(new List<DomainUser> { new() { DomainUserId = alreadyAssigned } });
        // ...but already has a reporting line.
        _managers.Setup(m => m.GetAssignedReportIds(It.IsAny<List<Guid>>()))
                 .ReturnsAsync(new List<Guid> { alreadyAssigned });

        await Assert.ThrowsAsync<BadRequestException>(
            () => _sut.AddReportToManager(Guid.NewGuid(), new AddReportToManagerDto { ReportIds = new List<Guid> { alreadyAssigned } }));

        _managers.Verify(m => m.AddReports(It.IsAny<Guid>(), It.IsAny<List<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task AddReportToManager_ReturnsTrue_WhenReportsExistAndAreUnassigned()
    {
        var report = Guid.NewGuid();
        _managers.Setup(m => m.GetManagerInfoAsync(It.IsAny<Guid>())).ReturnsAsync(new Manager());
        _users.Setup(u => u.AllDomainUsersExistAsync(It.IsAny<List<Guid>>()))
              .ReturnsAsync(new List<DomainUser> { new() { DomainUserId = report } });
        _managers.Setup(m => m.GetAssignedReportIds(It.IsAny<List<Guid>>())).ReturnsAsync(new List<Guid>());
        _managers.Setup(m => m.AddReports(It.IsAny<Guid>(), It.IsAny<List<Guid>>())).ReturnsAsync(true);

        var result = await _sut.AddReportToManager(Guid.NewGuid(), new AddReportToManagerDto { ReportIds = new List<Guid> { report } });

        Assert.True(result);
        _managers.Verify(m => m.AddReports(It.IsAny<Guid>(), It.IsAny<List<Guid>>()), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeInfo_Throws_WhenEmployeeNotFound()
    {
        _employees.Setup(e => e.GetEmployeeInfoAsync(It.IsAny<Guid>())).ReturnsAsync((EmployeeEntity?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetEmployeeInfo(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetManagerInfo_Throws_WhenManagerNotFound()
    {
        _managers.Setup(m => m.GetManagerInfoAsync(It.IsAny<Guid>())).ReturnsAsync((Manager?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetManagerInfo(Guid.NewGuid()));
    }

    // ---- Info mapping (real profile, so the response shape is covered) ----

    private EmployeeManagerService SutWithRealMapper() => new(
        _employees.Object, _managers.Object, _departments.Object,
        _organizations.Object, _users.Object,
        new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>(), NullLoggerFactory.Instance).CreateMapper(),
        NullLogger<EmployeeManagerService>.Instance);

    private static DomainUser Person(string first, string last) => new()
    {
        DomainUserId = Guid.NewGuid(),
        FirstName = first,
        LastName = last,
        Email = $"{first.ToLower()}@example.com",
        TenantId = 3,
        Tenant = new Tenant { TenantId = 3, Name = "Acme" },
    };

    [Fact]
    public async Task GetEmployeeInfo_ReturnsPersonAndOrganization()
    {
        var employeeId = Guid.NewGuid();
        _employees.Setup(e => e.GetEmployeeInfoAsync(employeeId)).ReturnsAsync(new EmployeeEntity
        {
            EmployeeId = employeeId,
            JobTitle = "Engineer",
            EmploymentStatus = "Active",
            DomainUser = Person("Alan", "Turing"),
            Department = new Department { Name = "Engineering", OrganizationId = 7 },
        });

        var result = await SutWithRealMapper().GetEmployeeInfo(employeeId);

        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal("Engineering", result.Department);
        Assert.Equal(7, result.OrganizationId);
        Assert.Equal("Alan", result.DomainUser.FirstName);
        Assert.Equal("Turing", result.DomainUser.LastName);
        Assert.Equal("alan@example.com", result.DomainUser.Email);
        Assert.Equal("Acme", result.DomainUser.TenantName);
    }

    [Fact]
    public async Task GetEmployeeInfo_ExposesManagerId_OnlyWhenTheEmployeeIsAManager()
    {
        var managerId = Guid.NewGuid();
        var plain = new EmployeeEntity { EmployeeId = Guid.NewGuid(), EmploymentStatus = "Active", DomainUser = Person("Alan", "Turing"), Department = new Department() };
        var manager = new EmployeeEntity { EmployeeId = Guid.NewGuid(), EmploymentStatus = "Active", DomainUser = Person("Ada", "Lovelace"), Department = new Department(), Manager = new Manager { ManagerId = managerId } };
        _employees.Setup(e => e.GetEmployeeInfoAsync(plain.EmployeeId)).ReturnsAsync(plain);
        _employees.Setup(e => e.GetEmployeeInfoAsync(manager.EmployeeId)).ReturnsAsync(manager);
        var sut = SutWithRealMapper();

        Assert.Null((await sut.GetEmployeeInfo(plain.EmployeeId)).ManagerId);
        Assert.Equal(managerId, (await sut.GetEmployeeInfo(manager.EmployeeId)).ManagerId);
    }

    [Fact]
    public async Task GetManagerInfo_ReturnsOrganizationHrDetailsAndReports()
    {
        var managerId = Guid.NewGuid();
        var managerEmployeeId = Guid.NewGuid();
        var report = Person("Grace", "Hopper");
        _managers.Setup(m => m.GetManagerInfoAsync(managerId)).ReturnsAsync(new Manager
        {
            ManagerId = managerId,
            EmployeeId = managerEmployeeId,
            DomainUser = Person("Ada", "Lovelace"),
            Department = new Department { Name = "Engineering", OrganizationId = 7 },
            Employee = new EmployeeEntity
            {
                EmployeeId = managerEmployeeId,
                JobTitle = "Head of Engineering",
                HireDate = new DateOnly(2024, 3, 1),
                Salary = 120000m,
                EmploymentStatus = "Active",
            },
            ReportingLines = { new ReportingLine { ReportId = report.DomainUserId, ManagerId = managerId, Report = report } },
        });

        var result = await SutWithRealMapper().GetManagerInfo(managerId);

        Assert.Equal(managerId, result.ManagerId);
        Assert.Equal(managerEmployeeId, result.EmployeeId);
        Assert.Equal("Head of Engineering", result.JobTitle);
        Assert.Equal(new DateOnly(2024, 3, 1), result.HireDate);
        Assert.Equal(120000m, result.Salary);
        Assert.Equal("Active", result.EmploymentStatus);
        Assert.Equal(7, result.OrganizationId);
        Assert.Equal("Ada", result.DomainUser.FirstName);
        var line = Assert.Single(result.ReportingLines);
        Assert.Equal("Grace", line.FirstName);
        Assert.Equal("Hopper", line.LastName);
    }
}
