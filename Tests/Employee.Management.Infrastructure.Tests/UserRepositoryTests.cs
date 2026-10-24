using AutoMapper;
using Employee.Management.Infrastructure.Repositories;
using Employee.Management.Models.DatabaseModels;
using Moq;

namespace Employee.Management.Infrastructure.Tests;

public class UserRepositoryTests
{
    private readonly EmployeeManagementDbContext _context = TestDb.NewContext();

    private UserRepository Repo() => new(_context, Mock.Of<IMapper>());

    [Fact]
    public async Task UpdateRoleAsync_SetsRoleAndUpdatedAt()
    {
        var domainUserId = Guid.NewGuid();
        var before = DateTime.UtcNow.AddDays(-1);
        _context.DomainUsers.Add(new DomainUser
        {
            DomainUserId = domainUserId,
            FirstName = "Alan",
            LastName = "Turing",
            Email = "alan@example.com",
            Role = "user",
            UpdatedAt = before,
        });
        await _context.SaveChangesAsync();

        var saved = await Repo().UpdateRoleAsync(domainUserId, "employee");

        Assert.Equal(1, saved);
        var user = _context.DomainUsers.Single(u => u.DomainUserId == domainUserId);
        Assert.Equal("employee", user.Role);
        Assert.True(user.UpdatedAt > before);
    }

    [Fact]
    public async Task UpdateRoleAsync_ReturnsZero_WhenUserDoesNotExist()
    {
        Assert.Equal(0, await Repo().UpdateRoleAsync(Guid.NewGuid(), "manager"));
    }

    [Theory]
    [InlineData("sys-admin")]
    [InlineData("company-admin")]
    public async Task UpdateRoleAsync_LeavesAdminRolesAlone(string adminRole)
    {
        var domainUserId = Guid.NewGuid();
        _context.DomainUsers.Add(new DomainUser
        {
            DomainUserId = domainUserId,
            FirstName = "Grace",
            LastName = "Hopper",
            Email = "grace@example.com",
            Role = adminRole,
        });
        await _context.SaveChangesAsync();

        Assert.Equal(0, await Repo().UpdateRoleAsync(domainUserId, "employee"));
        Assert.Equal(adminRole, _context.DomainUsers.Single(u => u.DomainUserId == domainUserId).Role);
    }
}
