using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface IDepartmentRepository
{
    Task<bool> CheckForExistingName(string name, int organizationId, Guid? departmentId = null);
    Task<List<DepartmentDto>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentInfoAsync(Guid departmentId);
    Task<int> CreateDepartmentAsync(Department department);
    Task<int> EditDepartmentAsync(Department department);
    Task<int> DeleteDepartmentAsync(Guid departmentId);
    Task<bool> HasMembersAsync(Guid departmentId);
}
