using Microsoft.EntityFrameworkCore;

namespace Employee.Management.Infrastructure.Tests;

// Every test calls NewContext() to get its OWN in-memory database (unique name),
// so tests never see each other's data. No Postgres needed — the queries run
// against EF Core's in-memory provider.
internal static class TestDb
{
    public static EmployeeManagementDbContext NewContext() =>
        new(new DbContextOptionsBuilder<EmployeeManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options);
}
