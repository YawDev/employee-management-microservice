using AutoMapper;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.Dtos;
using Microsoft.EntityFrameworkCore;

// 'Employee' is both a namespace root and an entity type — alias the entity to disambiguate.
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Infrastructure.Repositories
{
    public class EmployeeRepository(EmployeeManagementDbContext context, IMapper mapper) : IEmployeeRepository
    {
        private readonly EmployeeManagementDbContext _context = context;
        private readonly IMapper _mapper = mapper;

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

        public Task<List<EmployeeDto>> GetAllEmployeesAsync()
        {
            return _context.Employees
                .Include(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(e => e.Department)
                .Select(e => _mapper.Map<EmployeeDto>(e))
                .ToListAsync();
        }

        public async Task<EmployeeEntity?> GetEmployeeInfoAsync(Guid employeeId)
        {
            return await _context.Employees.AsNoTracking()
                .Include(e => e.DomainUser).ThenInclude(d => d.Tenant)
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }
    }
}
