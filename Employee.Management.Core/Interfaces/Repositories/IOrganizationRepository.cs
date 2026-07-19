using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface IOrganizationRepository
{
    Task<bool> CheckForExistingName(string name, int tenantId, int? organizationId = null);
    Task<List<OrganizationDto>> GetAllOrganizationsAsync();
    Task<Organization?> GetOrganizationInfoAsync(int organizationId);
    Task<int> CreateOrganizationAsync(Organization organization);
    Task<int> EditOrganizationAsync(Organization organization);
    Task<int> DeleteOrganizationAsync(int organizationId);
    Task<bool> HasDepartmentsAsync(int organizationId);
}
