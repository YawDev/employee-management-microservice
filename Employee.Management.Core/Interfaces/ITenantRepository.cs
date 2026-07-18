using Employee.Management.Models.Dtos;

namespace Employee.Management.Core.Interfaces;

public interface ITenantRepository
{
    Task<TenantDTO> GetTenantInfoAsync(int tenantId);
    Task<int> CreateTenantAsync(TenantDTO tenant);

    Task<List<TenantDTO>> GetAllTenantsAsync();

}
