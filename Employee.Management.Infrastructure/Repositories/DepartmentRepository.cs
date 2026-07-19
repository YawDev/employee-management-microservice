using AutoMapper;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Employee.Management.Infrastructure.Repositories
{
    public class DepartmentRepository(EmployeeManagementDbContext context, IMapper mapper) : IDepartmentRepository
    {
        private readonly EmployeeManagementDbContext _context = context;
        private readonly IMapper _mapper = mapper;

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

        public Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            return _context.Departments
                .Select(d => _mapper.Map<DepartmentDto>(d))
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentInfoAsync(Guid departmentId)
        {
            return await _context.Departments.FindAsync(departmentId);
        }

        // Any employees or managers still attached to the department.
        public async Task<bool> HasMembersAsync(Guid departmentId)
        {
            return await _context.Employees.AnyAsync(e => e.DepartmentId == departmentId)
                || await _context.Managers.AnyAsync(m => m.DepartmentId == departmentId);
        }
    }
}
