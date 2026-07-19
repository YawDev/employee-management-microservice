using AutoMapper;
using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.Dtos;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.DatabaseModels;
using Microsoft.Extensions.Logging;

namespace Employee.Management.Core.BusinessContext
{
    public class TenantService(ITenantRepository tenantRepository, IMapper mapper, ILogger<TenantService> logger) : ITenantService
    {
        private readonly ITenantRepository _tenantRepository = tenantRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<TenantService> _logger = logger;

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

            var created = await _tenantRepository.CreateTenantAsync(tenantEntity) > 0;
            _logger.LogInformation("Created tenant {TenantName} (success: {Created})", tenant.Name, created);
            return created;
        }

        public async Task<bool> DeleteTenant(int tenantId)
        {
            var existingTenant = await _tenantRepository.GetTenantInfoAsync(tenantId);
            if(existingTenant == null)
                throw new Exception("Tenant not found.");

            var deleted = await _tenantRepository.DeleteTenantAsync(tenantId) > 0;
            _logger.LogInformation("Deleted tenant {TenantId} (success: {Deleted})", tenantId, deleted);
            return deleted;
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

            var updated = await _tenantRepository.EditTenantAsync(existingTenant) > 0;
            _logger.LogInformation("Edited tenant {TenantId}", tenantId);
            return updated;
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