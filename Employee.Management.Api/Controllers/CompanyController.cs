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
    [Route("org")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IEmployeeManagerService _employeeManagerService;
        public CompanyController(ICompanyService companyService, IEmployeeManagerService employeeManagerService)
        {
            _companyService = companyService;
            _employeeManagerService = employeeManagerService;
        }

        [HttpGet("get-all-departments/{companyId}")]
        public async Task<IActionResult> GetAllDepartmentsForOrganization(int companyId)
        {
            var departments = await _companyService.GetAllDepartmentsForOrganization(companyId);
            return Ok(departments);
        }

        [HttpGet("get-department-info/{companyId}/{departmentId}")]
        public async Task<IActionResult> GetDepartmentInfoForOrganization(int companyId, Guid departmentId)
        {
            var department = await _companyService.GetDepartmentInfoForOrganization(companyId, departmentId);
            return Ok(department);
        }

        [HttpPost("create-department/{companyId}")]
        public async Task<IActionResult> CreateDepartmentForOrganization(int companyId, [FromBody] SaveDepartmentDto department)
        {
            var isSuccess = await _companyService.CreateDepartmentForOrganization(companyId, department);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPut("edit-department/{companyId}/{departmentId}")]
        public async Task<IActionResult> EditDepartmentForOrganization(int companyId, Guid departmentId, [FromBody] SaveDepartmentDto department)
        {
            var isSuccess = await _companyService.EditDepartmentForOrganization(companyId, departmentId, department);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPost("create-manager/{companyId}")]
        public async Task<IActionResult> CreateManagerForOrganization(int companyId, [FromBody] SaveManagerDto manager)
        {
            var isSuccess = await _employeeManagerService.CreateManagerForOrganization(manager, companyId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPut("edit-manager/{companyId}/{managerId}")]
        public async Task<IActionResult> EditManagerForOrganization(int companyId, Guid managerId, [FromBody] SaveManagerDto manager)
        {
            var isSuccess = await _employeeManagerService.EditManagerForOrganization(managerId, manager, companyId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPost("create-employee/{companyId}")]
        public async Task<IActionResult> CreateEmployeeForOrganization(int companyId, [FromBody] SaveEmployeeDto employeeDto)
        {
            var isSuccess = await _employeeManagerService.CreateEmployeeForOrganization(employeeDto, companyId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPut("edit-employee/{companyId}/{employeeId}")]
        public async Task<IActionResult> EditEmployeeForOrganization(int companyId, Guid employeeId, [FromBody] SaveEmployeeDto employeeDto)
        {
            var isSuccess = await _employeeManagerService.EditEmployeeForOrganization(employeeId, employeeDto, companyId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpPost("add-report-for-manager/{companyId}")]
        public async Task<IActionResult> AddReportForManager(int companyId, [FromBody] AddReportToManagerDto request)
        {
            var isSuccess = await _employeeManagerService.AddReportToManagerForOrganization(request.ManagerId, request.ReportIds, companyId);
            return Ok(new { IsSaved = isSuccess });
        }

        [HttpDelete("remove-report-for-manager/{companyId}")]
        public async Task<IActionResult> RemoveReportForManager(int companyId, [FromBody] AddReportToManagerDto request)
        {
            var isSuccess = await _employeeManagerService.RemoveReportFromManagerForOrganization(request.ManagerId, request.ReportIds, companyId);
            return Ok(new { IsSaved = isSuccess });
        }

    }
}
