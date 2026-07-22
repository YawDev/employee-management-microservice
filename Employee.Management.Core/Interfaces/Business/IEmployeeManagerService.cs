using Employee.Management.Models.Dtos;
using Employee.Management.Models.Dtos.RequestDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee.Management.Core.Interfaces.Business
{
    // People designations: Employee (ICs) + Manager operations.
    public interface IEmployeeManagerService
    {
        public Task<List<EmployeeDto>> GetAllEmployees();
        public Task<EmployeeDto> GetEmployeeInfo(Guid employeeId);
        public Task<bool> CreateEmployee(SaveEmployeeDto employee);
        public Task<bool> EditEmployee(Guid employeeId, SaveEmployeeDto employee);
        public Task<bool> DeleteEmployee(Guid employeeId);
        public Task<bool> AddReportToManager(Guid managerId, AddReportToManagerDto request);
        public Task<List<ManagerDto>> GetAllManagers();
        public Task<ManagerDto> GetManagerInfo(Guid managerId);
        public Task<bool> CreateManager(SaveManagerDto manager);
        public Task<bool> EditManager(Guid managerId, SaveManagerDto manager);
        public Task<bool> DeleteManager(Guid managerId);
    }
}
