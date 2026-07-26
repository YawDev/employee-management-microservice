using Employee.Management.Models.DatabaseModels;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface IManagerRepository
{
    Task<bool> CheckForExistingDomainUser(Guid domainUserId, Guid? managerId = null);
    Task<List<Manager>> GetAllManagersAsync();
    Task<List<Manager>> GetAllManagersForOrganizationAsync(int organizationId);
    Task<Manager?> GetManagerInfoAsync(Guid managerId);
    Task<int> CreateManagerAsync(Manager manager);
    Task<int> EditManagerAsync(Manager manager);
    Task<int> DeleteManagerAsync(Guid managerId);
    Task<bool> HasReportsAsync(Guid managerId);
    Task<List<Guid>> GetAssignedReportIds(List<Guid> reportIds);
    Task<bool> AddReports(Guid managerId, List<Guid> reportIds);
    Task<bool> RemoveReports(Guid managerId, List<Guid> reportIds);
    Task<List<ReportingLine>> GetAllReportsForManager(Guid managerId);
    Task<ReportingLine?> GetReportInfoForManager(Guid managerId, Guid reportId);
}
