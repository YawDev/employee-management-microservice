using Employee.Management.Infrastructure.Repositories;
using Employee.Management.Models.DatabaseModels;
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Infrastructure.Tests;

public class DepartmentRepositoryTests
{
    private readonly EmployeeManagementDbContext _context = TestDb.NewContext();

    [Fact]
    public async Task CreateDepartmentAsync_PersistsTheDepartment()
    {
        var repo = new DepartmentRepository(_context);

        var rows = await repo.CreateDepartmentAsync(
            new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 1, Name = "Engineering" });

        Assert.Equal(1, rows);
        Assert.Single(_context.Departments);
    }

    [Fact]
    public async Task CheckForExistingName_IsScopedToTheOrganization()
    {
        var repo = new DepartmentRepository(_context);
        _context.Departments.Add(new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 1, Name = "Engineering" });
        await _context.SaveChangesAsync();

        Assert.True(await repo.CheckForExistingName("Engineering", organizationId: 1));
        Assert.False(await repo.CheckForExistingName("Engineering", organizationId: 2)); // same name, different org is fine
    }

    [Fact]
    public async Task HasMembersAsync_ReturnsTrue_WhenAnEmployeeIsAttached()
    {
        var repo = new DepartmentRepository(_context);
        var departmentId = Guid.NewGuid();
        _context.Departments.Add(new Department { DepartmentId = departmentId, OrganizationId = 1, Name = "Engineering" });
        _context.Employees.Add(new EmployeeEntity { EmployeeId = Guid.NewGuid(), DomainUserId = Guid.NewGuid(), DepartmentId = departmentId, EmploymentStatus = "Active" });
        await _context.SaveChangesAsync();

        Assert.True(await repo.HasMembersAsync(departmentId));
        Assert.False(await repo.HasMembersAsync(Guid.NewGuid())); // empty department
    }
}
