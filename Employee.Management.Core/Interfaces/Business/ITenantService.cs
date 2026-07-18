using Employee.Management.Models.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using Employee.Management.Models.Dtos.RequestDtos;
namespace Employee.Management.Core.Interfaces.Business
{
    public interface ITenantService
    {
        public Task<List<TenantDto>> GetAllTenants();
        public Task<TenantDto> GetTenantInfo(int tenantId);
        public Task<bool> EditTenant(int tenantId, TenantDto tenant);
        public Task<bool> CreateTenant(CreateTenantDto tenant);
        public Task<bool> DeleteTenant(int tenantId);

    }
}