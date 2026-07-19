using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface IManagerRepository
{
    Task<bool> CheckForExistingDomainUser(Guid domainUserId, Guid? managerId = null);
    Task<List<ManagerDto>> GetAllManagersAsync();
    Task<Manager?> GetManagerInfoAsync(Guid managerId);
    Task<int> CreateManagerAsync(Manager manager);
    Task<int> EditManagerAsync(Manager manager);
    Task<int> DeleteManagerAsync(Guid managerId);
    Task<bool> HasReportsAsync(Guid managerId);
}
