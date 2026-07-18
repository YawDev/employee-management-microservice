using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface ITenantRepository
{
    Task<bool> CheckForExistingName(string name, int? tenantId = null);
    Task<Tenant> GetTenantInfoAsync(int tenantId);
    Task<int> CreateTenantAsync(Tenant tenant);
    Task<List<TenantDto>> GetAllTenantsAsync();
    Task<int> DeleteTenantAsync(int tenantId);
    Task<int> EditTenantAsync(Tenant tenant);
}
