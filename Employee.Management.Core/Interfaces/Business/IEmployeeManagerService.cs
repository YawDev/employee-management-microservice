using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;

namespace Employee.Management.Core.Interfaces.Business
{
    // People designations: Employee (ICs) + Manager operations.
    public interface IEmployeeManagerService
    {
        #region System User Permissions
        public Task<List<EmployeeResponseDto>> GetAllEmployees();
        public Task<EmployeeResponseDto> GetEmployeeInfo(Guid employeeId);
        public Task<bool> CreateEmployee(SaveEmployeeDto employee);
        public Task<bool> EditEmployee(Guid employeeId, SaveEmployeeDto employee);
        public Task<bool> DeleteEmployee(Guid employeeId);
        public Task<bool> AddReportToManager(Guid managerId, AddReportToManagerDto request);
        public Task<bool> RemoveReportFromManager(Guid managerId, Guid reportId);
        public Task<List<ManagerInfoResponseDto>> GetAllManagers();
        public Task<ManagerInfoResponseDto> GetManagerInfo(Guid managerId);
        public Task<bool> CreateManager(SaveManagerDto manager);
        public Task<bool> EditManager(Guid managerId, SaveManagerDto manager);
        public Task<bool> DeleteManager(Guid managerId);
        #endregion

        #region Company Admin Permissions
        public Task<List<ReportLineResponseDto>> GetAllReportsForManager(Guid managerId, int OrganizationId);
        public Task<ReportLineResponseDto> GetReportInfoForManager(Guid managerId, Guid reportId, int OrganizationId);
        public Task<bool> AddReportToManager(Guid managerId, List<Guid> reportIds, int OrganizationId);
        public Task<bool> RemoveReportFromManager(Guid managerId, List<Guid> reportIds, int OrganizationId);

        public Task<List<ManagerInfoResponseDto>> GetAllManagersForOrganization(int OrganizationId);
        public Task<ManagerInfoResponseDto> GetManagerInfoForOrganization(Guid managerId, int OrganizationId);
        public Task<bool> CreateManagerForOrganization(SaveManagerDto manager, int OrganizationId);
        public Task<bool> EditManagerForOrganization(Guid managerId, SaveManagerDto manager, int OrganizationId);

        public Task<List<EmployeeResponseDto>> GetAllEmployeesForOrganization(int OrganizationId);
        public Task<EmployeeResponseDto> GetEmployeeInfoForOrganization(Guid employeeId, int OrganizationId);
        public Task<bool> CreateEmployeeForOrganization(SaveEmployeeDto employee, int OrganizationId);
        public Task<bool> EditEmployeeForOrganization(Guid employeeId, SaveEmployeeDto employee, int OrganizationId);

        public Task<bool> AddReportToManagerForOrganization(Guid managerId, List<Guid> reportIds, int OrganizationId);
        public Task<bool> RemoveReportFromManagerForOrganization(Guid managerId, List<Guid> reportIds, int OrganizationId);

        #endregion
    }
}
