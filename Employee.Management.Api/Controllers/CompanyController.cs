using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Models.Dtos.RequestDtos;

namespace Employee.Management.Api.Controllers
{
    [Authorize(Policy = "CompanyPermission")] 
    [ApiController]
    [Route("company-admin-api")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IEmployeeManagerService _employeeManagerService;
        public CompanyController(ICompanyService companyService, IEmployeeManagerService employeeManagerService)
        {
            _companyService = companyService;
            _employeeManagerService = employeeManagerService;
        }

        [HttpGet("get-all-departments-for-organization/{organizationId}")]
        public async Task<IActionResult> GetAllDepartmentsForOrganization(int organizationId)
        {
            var departments = await _companyService.GetAllDepartmentsForOrganization(organizationId);
            return Ok(departments);
        }

        [HttpGet("get-department-info-for-organization/{organizationId}/{departmentId}")]
        public async Task<IActionResult> GetDepartmentInfoForOrganization(int organizationId, Guid departmentId)
        {
            var department = await _companyService.GetDepartmentInfoForOrganization(organizationId, departmentId);
            return Ok(department);
        }

        [HttpPost("create-department-for-organization/{organizationId}")]
        public async Task<IActionResult> CreateDepartmentForOrganization(int organizationId, [FromBody] SaveDepartmentDto department)
        {
            var isSuccess = await _companyService.CreateDepartmentForOrganization(organizationId, department);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPut("edit-department-for-organization/{organizationId}/{departmentId}")]
        public async Task<IActionResult> EditDepartmentForOrganization(int organizationId, Guid departmentId, [FromBody] SaveDepartmentDto department)
        {
            var isSuccess = await _companyService.EditDepartmentForOrganization(organizationId, departmentId, department);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPost("create-manager-for-organization/{organizationId}")]
        public async Task<IActionResult> CreateManagerForOrganization(int organizationId, [FromBody] SaveManagerDto manager)
        {
            var isSuccess = await _employeeManagerService.CreateManagerForOrganization(manager, organizationId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPut("edit-manager-for-organization/{organizationId}/{managerId}")]
        public async Task<IActionResult> EditManagerForOrganization(int organizationId, Guid managerId, [FromBody] SaveManagerDto manager)
        {
            var isSuccess = await _employeeManagerService.EditManagerForOrganization(managerId, manager, organizationId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPost("create-employee-for-organization/{organizationId}")]
        public async Task<IActionResult> CreateEmployeeForOrganization(int organizationId, [FromBody] SaveEmployeeDto employeeDto)
        {
            var isSuccess = await _employeeManagerService.CreateEmployeeForOrganization(employeeDto, organizationId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPut("edit-employee-for-organization/{organizationId}/{employeeId}")]
        public async Task<IActionResult> EditEmployeeForOrganization(int organizationId, Guid employeeId, [FromBody] SaveEmployeeDto employeeDto)
        {
            var isSuccess = await _employeeManagerService.EditEmployeeForOrganization(employeeId, employeeDto, organizationId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPost("add-report-for-manager/org/{organizationId}")]
        public async Task<IActionResult> AddReportForManager(int organizationId, [FromBody] AddReportToManagerDto request)
        {
            var isSuccess = await _employeeManagerService.AddReportToManagerForOrganization(request.ManagerId, request.ReportIds, organizationId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpDelete("remove-report-for-manager/org/{organizationId}")]
        public async Task<IActionResult> RemoveReportForManager(int organizationId, [FromBody] AddReportToManagerDto request)
        {
            var isSuccess = await _employeeManagerService.RemoveReportFromManagerForOrganization(request.ManagerId, request.ReportIds, organizationId);
            return Ok(new { IsSaved = isSuccess });
        }

    }
}