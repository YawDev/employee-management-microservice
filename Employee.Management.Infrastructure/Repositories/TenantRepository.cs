using AutoMapper;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Employee.Management.Infrastructure.Repositories
{

    public class TenantRepository(EmployeeManagementDbContext context, IMapper mapper) : ITenantRepository
    {
        private readonly EmployeeManagementDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public Task<bool> CheckForExistingName(string tenantName, int? tenantId = null)
        {
            return _context.Tenants.AnyAsync(t => t.Name == tenantName && (!tenantId.HasValue || t.TenantId != tenantId.Value));
        }

        public async Task<int> CreateTenantAsync(Tenant tenant)
        {
            await _context.AddAsync(tenant);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteTenantAsync(int tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return 0;
            _context.Tenants.Remove(tenant);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> EditTenantAsync(Tenant tenant)
        {
            _context.Tenants.Update(tenant);
            return await _context.SaveChangesAsync();
        }

        public Task<List<TenantDto>> GetAllTenantsAsync()
        {
            return _context.Tenants
                .Select(t => _mapper.Map<TenantDto>(t))
                .ToListAsync(); 
        }

        public async Task<Tenant> GetTenantInfoAsync(int tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            return tenant;
        }
    }

}