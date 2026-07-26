using Employee.Management.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

// 'Employee' is both a namespace root and an entity type — alias the entity to disambiguate.
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Infrastructure.Repositories
{
    public class EmployeeRepository(EmployeeManagementDbContext context) : IEmployeeRepository
    {
        private readonly EmployeeManagementDbContext _context = context;

        public Task<bool> CheckForExistingDomainUser(Guid domainUserId, Guid? employeeId = null)
        {
            return _context.Employees.AnyAsync(e =>
                e.DomainUserId == domainUserId
                && (!employeeId.HasValue || e.EmployeeId != employeeId.Value));
        }

        public async Task<int> CreateEmployeeAsync(EmployeeEntity employee)
        {
            await _context.AddAsync(employee);
            return await _context.SaveChangesAsync();
        }

        // Also carries soft-deletes (off-boarding flips EmploymentStatus / EndDate) —
        // employees are never hard-deleted, so there is no DeleteEmployeeAsync.
        public async Task<int> EditEmployeeAsync(EmployeeEntity employee)
        {
            _context.Employees.Update(employee);
            return await _context.SaveChangesAsync();
        }

        // Scoped write: only persists when the employee still belongs to the organization,
        // so a Company Admin can't edit an employee outside their org.
        public async Task<int> EditEmployeeForOrganizationAsync(int organizationId, EmployeeEntity employee)
        {
            var belongsToOrg = await _context.Employees.AnyAsync(e =>
                e.EmployeeId == employee.EmployeeId && e.Department.OrganizationId == organizationId);
            if (!belongsToOrg) return 0;

            _context.Employees.Update(employee);
            return await _context.SaveChangesAsync();
        }

        public Task<List<EmployeeEntity>> GetAllEmployeesAsync()
        {
            return _context.Employees.AsNoTracking()
                .Include(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(e => e.Department)
                .ToListAsync();
        }

        public Task<List<EmployeeEntity>> GetAllEmployeesForOrganizationAsync(int organizationId)
        {
            return _context.Employees.AsNoTracking()
                .Where(e => e.Department.OrganizationId == organizationId)
                .Include(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(e => e.Department)
                .ToListAsync();
        }

        public async Task<EmployeeEntity?> GetEmployeeInfoAsync(Guid employeeId)
        {
            return await _context.Employees.AsNoTracking()
                .Include(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<EmployeeEntity?> GetEmployeeInfoForOrganizationAsync(int organizationId, Guid employeeId)
        {
            return await _context.Employees.AsNoTracking()
                .Include(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Department.OrganizationId == organizationId);
        }
    }
}
