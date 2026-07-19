using Employee.Management.Models.Dtos;

// 'Employee' is both a namespace root and an entity type — alias the entity to disambiguate.
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<bool> CheckForExistingDomainUser(Guid domainUserId, Guid? employeeId = null);
    Task<List<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeEntity?> GetEmployeeInfoAsync(Guid employeeId);
    Task<int> CreateEmployeeAsync(EmployeeEntity employee);
    Task<int> EditEmployeeAsync(EmployeeEntity employee);
}
