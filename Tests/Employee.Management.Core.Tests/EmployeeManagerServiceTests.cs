using AutoMapper;
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

        var result = await _sut.CreateEmployee(new SaveEmployeeDto { DomainUserId = Guid.NewGuid(), DepartmentId = Guid.NewGuid() });

        Assert.True(result);
        _employees.Verify(e => e.CreateEmployeeAsync(It.IsAny<EmployeeEntity>()), Times.Once);
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
}
