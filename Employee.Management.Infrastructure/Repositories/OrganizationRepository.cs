using AutoMapper;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos.ResponseDtos;
using Microsoft.EntityFrameworkCore;

namespace Employee.Management.Infrastructure.Repositories
{
    public class OrganizationRepository(EmployeeManagementDbContext context, IMapper mapper) : IOrganizationRepository
    {
        private readonly EmployeeManagementDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public Task<bool> CheckForExistingName(string name, int tenantId, int? organizationId = null)
        {
            return _context.Organizations.AnyAsync(o =>
                o.Name == name
                && o.TenantId == tenantId
                && (!organizationId.HasValue || o.OrganizationId != organizationId.Value));
        }

        public async Task<int> CreateOrganizationAsync(Organization organization)
        {
            await _context.AddAsync(organization);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteOrganizationAsync(int organizationId)
        {
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null) return 0;
            _context.Organizations.Remove(organization);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> EditOrganizationAsync(Organization organization)
        {
            _context.Organizations.Update(organization);
            return await _context.SaveChangesAsync();
        }

        public Task<List<OrganizationResponseDto>> GetAllOrganizationsAsync()
        {
            return _context.Organizations
            .Include(o => o.Departments)
            .ThenInclude(d => d.Employees).ThenInclude(e => e.DomainUser).ThenInclude(d => d.Tenant)
            .Include(o => o.Departments)
            .Include(o => o.Departments).ThenInclude(d => d.Managers).ThenInclude(r => r.DomainUser).ThenInclude(d => d.Tenant)
            .Include(o => o.Departments).ThenInclude(d => d.Managers).ThenInclude(r => r.ReportingLines)
                .Select(o => _mapper.Map<OrganizationResponseDto>(o))
                .ToListAsync();
        }

        public async Task<Organization?> GetOrganizationInfoAsync(int organizationId)
        {
            return await _context.Organizations.AsNoTracking()
                .Include(o => o.Departments).ThenInclude(d => d.Employees).ThenInclude(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(o => o.Departments).ThenInclude(d => d.Managers)
                .ThenInclude(m => m.DomainUser).ThenInclude(d => d.Tenant)
                .Include(o => o.Departments).ThenInclude(d => d.Managers)
                .ThenInclude(m => m.ReportingLines).ThenInclude(r => r.Report)
                .FirstOrDefaultAsync(o => o.OrganizationId == organizationId);
        }

        public Task<bool> HasDepartmentsAsync(int organizationId)
        {
            return _context.Departments.AnyAsync(d => d.OrganizationId == organizationId);
        }
    }
}
