using Employee.Management.Infrastructure.Repositories;
using Employee.Management.Models.DatabaseModels;
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Infrastructure.Tests;

public class EmployeeRepositoryTests
{
    private readonly EmployeeManagementDbContext _context = TestDb.NewContext();

    [Fact]
    public async Task CreateEmployeeAsync_ThenGetEmployeeInfoAsync_RoundTripsTheEmployee()
    {
        var repo = new EmployeeRepository(_context);
        var employeeId = Guid.NewGuid();

        var rows = await repo.CreateEmployeeAsync(new EmployeeEntity
        {
            EmployeeId = employeeId,
            EmploymentStatus = "Active",
            JobTitle = "Engineer",
            Department = new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 1, Name = "Engineering" },
            DomainUser = Person()
        });
        var fetched = await repo.GetEmployeeInfoAsync(employeeId);

        Assert.True(rows > 0);
        Assert.NotNull(fetched);
        Assert.Equal("Engineer", fetched!.JobTitle);
    }

    [Fact]
    public async Task CheckForExistingDomainUser_ReturnsTrue_OnlyWhenThatPersonHasAnEmployeeRecord()
    {
        var repo = new EmployeeRepository(_context);
        var domainUserId = Guid.NewGuid();
        _context.Employees.Add(new EmployeeEntity
        {
            EmployeeId = Guid.NewGuid(),
            DomainUserId = domainUserId,
            DepartmentId = Guid.NewGuid(),
            EmploymentStatus = "Active"
        });
        await _context.SaveChangesAsync();

        Assert.True(await repo.CheckForExistingDomainUser(domainUserId));
        Assert.False(await repo.CheckForExistingDomainUser(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllEmployeesForOrganizationAsync_ReturnsOnlyEmployeesInThatOrg()
    {
        var repo = new EmployeeRepository(_context);
        // Employees eager-load DomainUser (+Tenant) and Department, so seed the whole person.
        var engDept = new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 1, Name = "Engineering" };
        _context.Employees.Add(new EmployeeEntity { EmployeeId = Guid.NewGuid(), EmploymentStatus = "Active", Department = engDept, DomainUser = Person() });
        _context.Employees.Add(new EmployeeEntity { EmployeeId = Guid.NewGuid(), EmploymentStatus = "Active", Department = new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 2, Name = "Sales" }, DomainUser = Person() });
        await _context.SaveChangesAsync();

        var result = await repo.GetAllEmployeesForOrganizationAsync(organizationId: 1);

        Assert.Single(result);
        Assert.Equal(engDept.DepartmentId, result[0].DepartmentId);
    }

    [Fact]
    public async Task GetEmployeeInfoForOrganizationAsync_ReturnsNull_WhenEmployeeIsInADifferentOrg()
    {
        var repo = new EmployeeRepository(_context);
        var employeeId = Guid.NewGuid();
        _context.Employees.Add(new EmployeeEntity
        {
            EmployeeId = employeeId,
            EmploymentStatus = "Active",
            Department = new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 1, Name = "Engineering" },
            DomainUser = Person()
        });
        await _context.SaveChangesAsync();

        Assert.Null(await repo.GetEmployeeInfoForOrganizationAsync(organizationId: 2, employeeId));   // wrong org
        Assert.NotNull(await repo.GetEmployeeInfoForOrganizationAsync(organizationId: 1, employeeId)); // right org
    }

    // A valid person: DomainUser + the Tenant it requires. Employees eager-load this graph,
    // so tests need a real one rather than a bare FK guid.
    private static DomainUser Person() => new()
    {
        DomainUserId = Guid.NewGuid(),
        FirstName = "Test",
        LastName = "Person",
        Email = "test@example.com",
        Role = "employee",
        Tenant = new Tenant { Name = "Acme" }
    };
}
