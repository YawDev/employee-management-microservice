using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<int> CreateAsync(DomainUser user);
    Task<int> CreateIdentityUserAsync(ApplicationUser user);
    Task<bool> DeleteAsync(Guid userId);
    Task<bool> DomainUserExistsAsync(Guid domainUserId);
    Task<bool> ExistsAsync(Guid userId);
    Task<IdentityUserDTO?> GetByEmailAsync(string email);
    Task<DomainUserDto?> GetByIdAsync(Guid userId);
    Task<ApplicationUser?> GetByUserNameAsync(string userName);
    Task<IdentityUserDTO?> GetIdentityUserInfoAsync(Guid id);
    Task<ApplicationUser> UpdateAsync(ApplicationUser user);
    Task<bool> ValidateCredentialsAsync(string userName, string passwordHash);
}
