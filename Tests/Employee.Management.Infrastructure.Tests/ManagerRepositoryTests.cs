using Employee.Management.Infrastructure.Repositories;
using Employee.Management.Models.DatabaseModels;

namespace Employee.Management.Infrastructure.Tests;

// Repository = data access. These tests seed rows into an in-memory DB, run the
// repository method, then read the DB back to prove it did the right thing.
public class ManagerRepositoryTests
{
    private readonly EmployeeManagementDbContext _context = TestDb.NewContext();

    [Fact]
    public async Task AddReports_CreatesOneReportingLinePerReportId()
    {
        var repo = new ManagerRepository(_context);
        var managerId = Guid.NewGuid();
        var reportA = Guid.NewGuid();
        var reportB = Guid.NewGuid();

        var result = await repo.AddReports(managerId, new List<Guid> { reportA, reportB });

        Assert.True(result);
        var lines = _context.ReportingLines.ToList();
        Assert.Equal(2, lines.Count);
        Assert.All(lines, line => Assert.Equal(managerId, line.ManagerId));
    }

    [Fact]
    public async Task GetAssignedReportIds_ReturnsOnlyIdsThatAlreadyReport()
    {
        var repo = new ManagerRepository(_context);
        var alreadyAssigned = Guid.NewGuid();
        var free = Guid.NewGuid();
        _context.ReportingLines.Add(new ReportingLine { ManagerId = Guid.NewGuid(), ReportId = alreadyAssigned });
        await _context.SaveChangesAsync();

        var result = await repo.GetAssignedReportIds(new List<Guid> { alreadyAssigned, free });

        Assert.Equal(new[] { alreadyAssigned }, result);
    }

    [Fact]
    public async Task RemoveReports_DeletesTheMatchingReportingLine()
    {
        var repo = new ManagerRepository(_context);
        var managerId = Guid.NewGuid();
        var report = Guid.NewGuid();
        _context.ReportingLines.Add(new ReportingLine { ManagerId = managerId, ReportId = report });
        await _context.SaveChangesAsync();

        var result = await repo.RemoveReports(managerId, new List<Guid> { report });

        Assert.True(result);
        Assert.Empty(_context.ReportingLines);
    }

    [Fact]
    public async Task CheckForExistingDomainUser_ReturnsTrue_OnlyWhenThatPersonIsAManager()
    {
        var repo = new ManagerRepository(_context);
        var domainUserId = Guid.NewGuid();
        _context.Managers.Add(new Manager { ManagerId = Guid.NewGuid(), DomainUserId = domainUserId, DepartmentId = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        Assert.True(await repo.CheckForExistingDomainUser(domainUserId));
        Assert.False(await repo.CheckForExistingDomainUser(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllManagersForOrganizationAsync_ReturnsOnlyManagersInThatOrg()
    {
        var repo = new ManagerRepository(_context);
        // A manager eager-loads its DomainUser (+Tenant) and Department, so seed the whole
        // person — otherwise EF's inner join for those required navs drops the row.
        var engDept = new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 1, Name = "Engineering" };
        _context.Managers.Add(new Manager { ManagerId = Guid.NewGuid(), Department = engDept, DomainUser = Person() });
        _context.Managers.Add(new Manager { ManagerId = Guid.NewGuid(), Department = new Department { DepartmentId = Guid.NewGuid(), OrganizationId = 2, Name = "Sales" }, DomainUser = Person() });
        await _context.SaveChangesAsync();

        var result = await repo.GetAllManagersForOrganizationAsync(organizationId: 1);

        Assert.Single(result);
        Assert.Equal(engDept.DepartmentId, result[0].DepartmentId);
    }

    // A valid person: DomainUser + the Tenant it requires. Managers/employees eager-load
    // this graph, so tests need a real one rather than a bare FK guid.
    private static DomainUser Person() => new()
    {
        DomainUserId = Guid.NewGuid(),
        FirstName = "Test",
        LastName = "Person",
        Email = "test@example.com",
        Role = "manager",
        Tenant = new Tenant { Name = "Acme" }
    };
}
