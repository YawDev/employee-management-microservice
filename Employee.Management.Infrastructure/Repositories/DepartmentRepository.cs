using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace Employee.Management.Infrastructure.Repositories
{
    public class DepartmentRepository(EmployeeManagementDbContext context) : IDepartmentRepository
    {
        private readonly EmployeeManagementDbContext _context = context;

        public Task<bool> CheckForExistingName(string name, int organizationId, Guid? departmentId = null)
        {
            return _context.Departments.AnyAsync(d =>
                d.Name == name
                && d.OrganizationId == organizationId
                && (!departmentId.HasValue || d.DepartmentId != departmentId.Value));
        }

        public async Task<int> CreateDepartmentAsync(Department department)
        {
            await _context.AddAsync(department);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteDepartmentAsync(Guid departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null) return 0;
            _context.Departments.Remove(department);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> EditDepartmentAsync(Department department)
        {
            _context.Departments.Update(department);
            return await _context.SaveChangesAsync();
        }

        // Identity resolution so each Employee/Manager's back-reference to its parent
        // Department resolves to the same instance — the flattened department name in the
        // response DTOs reads from that nav and would NRE under plain AsNoTracking.
        public Task<List<Department>> GetAllDepartmentsAsync()
        {
            return _context.Departments.AsNoTrackingWithIdentityResolution()
                .Include(d => d.Employees).ThenInclude(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(d => d.Managers).ThenInclude(m => m.DomainUser).ThenInclude(d => d.Tenant)
                .ToListAsync();
        }

        public Task<List<Department>> GetAllDepartmentsForOrganizationAsync(int organizationId)
        {
            return _context.Departments.AsNoTrackingWithIdentityResolution()
                .Where(d => d.OrganizationId == organizationId)
                .Include(d => d.Employees).ThenInclude(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(d => d.Managers).ThenInclude(m => m.DomainUser).ThenInclude(d => d.Tenant)
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentInfoAsync(Guid departmentId)
        {
            return await _context.Departments.AsNoTrackingWithIdentityResolution()
                .Include(d => d.Employees).ThenInclude(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(d => d.Managers).ThenInclude(m => m.DomainUser).ThenInclude(d => d.Tenant)
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
        }

        // Any employees or managers still attached to the department.
        public async Task<bool> HasMembersAsync(Guid departmentId)
        {
            return await _context.Employees.AnyAsync(e => e.DepartmentId == departmentId)
                || await _context.Managers.AnyAsync(m => m.DepartmentId == departmentId);
        }
    }
}
