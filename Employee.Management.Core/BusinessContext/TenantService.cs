using AutoMapper;
using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.Dtos;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.DatabaseModels;

namespace Employee.Management.Core.BusinessContext
{
    public class TenantService(ITenantRepository tenantRepository, IMapper mapper) : ITenantService
    {
        private readonly ITenantRepository _tenantRepository = tenantRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> CreateTenant(CreateTenantDto tenant)
        {

            var existingName = await _tenantRepository.CheckForExistingName(tenant.Name);
            
            if(existingName)
                throw new Exception("Tenant name already exists.");

            var tenantEntity = new Tenant
            {
                Name = tenant.Name,
                Logo = tenant.Logo
            };

            return await _tenantRepository.CreateTenantAsync(tenantEntity) > 0;
        }

        public async Task<bool> DeleteTenant(int tenantId)
        {
            var existingTenant = await _tenantRepository.GetTenantInfoAsync(tenantId);
            if(existingTenant == null)
                throw new Exception("Tenant not found.");

            return await _tenantRepository.DeleteTenantAsync(tenantId) > 0; 
        }

        public async Task<bool> EditTenant(int tenantId, EditTenantDto tenant)
        {
            var existingTenant = await _tenantRepository.GetTenantInfoAsync(tenantId);
            if (existingTenant == null)
                throw new Exception("Tenant not found.");
                
            if (await _tenantRepository.CheckForExistingName(tenant.Name, tenantId))
                throw new Exception("Tenant name already exists.");

            existingTenant.Name = tenant.Name;
            existingTenant.Logo = tenant.Logo;

            return await _tenantRepository.EditTenantAsync(existingTenant) > 0;
        }

        public async Task<List<TenantDto>> GetAllTenants()
        {
            return await _tenantRepository.GetAllTenantsAsync();
        }

        public async Task<TenantDto> GetTenantInfo(int tenantId)
        {
            var tenant = await _tenantRepository.GetTenantInfoAsync(tenantId);
            if(tenant == null)
                throw new Exception("Tenant not found.");

            return _mapper.Map<TenantDto>(tenant);
        }
    }
}