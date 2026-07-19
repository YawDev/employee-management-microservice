using AutoMapper;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;
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

        public Task<List<OrganizationDto>> GetAllOrganizationsAsync()
        {
            return _context.Organizations
                .Select(o => _mapper.Map<OrganizationDto>(o))
                .ToListAsync();
        }

        public async Task<Organization?> GetOrganizationInfoAsync(int organizationId)
        {
            return await _context.Organizations.FindAsync(organizationId);
        }

        public Task<bool> HasDepartmentsAsync(int organizationId)
        {
            return _context.Departments.AnyAsync(d => d.OrganizationId == organizationId);
        }
    }
}
