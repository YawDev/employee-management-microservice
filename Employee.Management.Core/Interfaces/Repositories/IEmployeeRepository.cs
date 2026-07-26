// 'Employee' is both a namespace root and an entity type — alias the entity to disambiguate.
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Core.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<bool> CheckForExistingDomainUser(Guid domainUserId, Guid? employeeId = null);
    Task<List<EmployeeEntity>> GetAllEmployeesAsync();
    Task<EmployeeEntity?> GetEmployeeInfoAsync(Guid employeeId);
    Task<int> CreateEmployeeAsync(EmployeeEntity employee);
    Task<int> EditEmployeeAsync(EmployeeEntity employee);

    #region  Company Admin Permissions
    Task<int> EditEmployeeForOrganizationAsync(int organizationId, EmployeeEntity employee);
    Task<List<EmployeeEntity>> GetAllEmployeesForOrganizationAsync(int organizationId);
    Task<EmployeeEntity?> GetEmployeeInfoForOrganizationAsync(int organizationId, Guid employeeId);
    #endregion
}
