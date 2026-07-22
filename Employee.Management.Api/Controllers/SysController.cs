using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Employee.Management.Api.Controllers
{
    [Authorize(Policy = "SystemAdmin")]
    [ApiController]
    [Route("sys-api")]
    public class SysController(ITenantService tenantService, ICompanyService companyService, IEmployeeManagerService employeeManagerService) : ControllerBase
    {
        private readonly ITenantService _tenantService = tenantService;
        private readonly ICompanyService _companyService = companyService;
        private readonly IEmployeeManagerService _employeeManagerService = employeeManagerService;

        [HttpGet("get-all-tenants")]

        public async Task<IActionResult> GetAllTenants()
        {
            var tenants = await _tenantService.GetAllTenants();
            return Ok(tenants);
        }

        [HttpGet("get-tenant-info/{id}")]
        public async Task<IActionResult> GetTenantInfo(int id)
        {
            var tenant = await _tenantService.GetTenantInfo(id);
            return Ok(tenant);
        }

        [HttpPost("create-tenant")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto createTenantDto)
        {
            var isSuccess = await _tenantService.CreateTenant(createTenantDto);
            return Ok(new SaveTenantDto { IsSaved = isSuccess });
        }

        [HttpPut("edit-tenant/{id}")]
        public async Task<IActionResult> EditTenant(int id, [FromBody] EditTenantDto editTenantDto)
        {

            var isSuccess = await _tenantService.EditTenant(id, editTenantDto);
            return Ok(new SaveTenantDto { IsSaved = isSuccess });
        }
        
        [HttpDelete("delete-tenant/{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var isSuccess = await _tenantService.DeleteTenant(id);
            return Ok(new DeleteTenantDto { IsDeleted = isSuccess });
        }

        [HttpGet("get-all-organizations")]
        public async Task<IActionResult> GetAllOrganizations()
        {
            var organizations = await _companyService.GetAllOrganizations();
            return Ok(new GetAllOrganizationResponseDto { Organizations = organizations });
        }

        [HttpGet("get-organization-info/{id}")]
        public async Task<IActionResult> GetOrganizationInfo(int id)
        {
            var organization = await _companyService.GetOrganizationInfo(id);
            return Ok(new GetOrganizationResponseDto { Organization = organization });
        }

        [HttpPost("create-organization")]
        public async Task<IActionResult> CreateOrganization([FromBody] SaveOrganizationDto saveOrganizationDto)
        {
            var isSuccess = await _companyService.CreateOrganization(saveOrganizationDto);
            return Ok(new SaveResponseDto { IsSaved = isSuccess });
        }

        [HttpPut("edit-organization/{id}")]
        public async Task<IActionResult> EditOrganization(int id, [FromBody] SaveOrganizationDto saveOrganizationDto)
        {
            var isSuccess = await _companyService.EditOrganization(id, saveOrganizationDto);
            return Ok(new SaveResponseDto { IsSaved = isSuccess });
        }

        [HttpDelete("delete-organization/{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            var isSuccess = await _companyService.DeleteOrganization(id);
            return Ok(new DeleteResponseDto { IsDeleted = isSuccess });
        }

        [HttpGet("get-all-departments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departments = await _companyService.GetAllDepartments();
            return Ok(departments);
        }

        [HttpGet("get-department-info/{id:guid}")]
        public async Task<IActionResult> GetDepartmentInfo(Guid id)
        {
            var department = await _companyService.GetDepartmentInfo(id);
            return Ok(department);
        }

        [HttpPost("create-department")]
        public async Task<IActionResult> CreateDepartment([FromBody] SaveDepartmentDto saveDepartmentDto)
        {
            var isSuccess = await _companyService.CreateDepartment(saveDepartmentDto);
            return Ok(new SaveResponseDto { IsSaved = isSuccess });
        }

        [HttpPut("edit-department/{id:guid}")]
        public async Task<IActionResult> EditDepartment(Guid id, [FromBody] SaveDepartmentDto saveDepartmentDto)
        {
            var isSuccess = await _companyService.EditDepartment(id, saveDepartmentDto);
            return Ok(new SaveResponseDto { IsSaved = isSuccess });
        }

        [HttpDelete("delete-department/{id:guid}")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var isSuccess = await _companyService.DeleteDepartment(id);
            return Ok(new DeleteResponseDto { IsDeleted = isSuccess });
        }

        [HttpGet("get-all-managers")]
        public async Task<IActionResult> GetAllManagers()
        {
            var managers = await _employeeManagerService.GetAllManagers();
            return Ok(managers);
        }

        [HttpPost("add-manager")]
        public async Task<IActionResult> AddManager([FromBody] SaveManagerDto request)
        {
            var result = await _employeeManagerService.CreateManager(request);
            return Ok(new SaveResponseDto { IsSaved = result });
        }

        [HttpDelete("delete-manager/{id:guid}")]
        public async Task<IActionResult> DeleteManager(Guid id)
        {
            var result = await _employeeManagerService.DeleteManager(id);
            return Ok(new DeleteResponseDto { IsDeleted = result });
        }

        [HttpPut("edit-manager/{id:guid}")]
        public async Task<IActionResult> EditManager(Guid id, [FromBody] SaveManagerDto request)
        {
            var result = await _employeeManagerService.EditManager(id, request);
            return Ok(new SaveResponseDto { IsSaved = result });
        }

        //TODO: Add implementation to for adding reports to a manager and removing reports from a manager.
        [HttpPost("add-report-to-manager")]
        public async Task<IActionResult> AddReportToManager([FromBody] AddReportToManagerDto request)
        {
            var result = await _employeeManagerService.AddReportToManager(request.ManagerId, request);
            return Ok(new SaveResponseDto { IsSaved = result });
        }

        [HttpGet("get-all-employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeManagerService.GetAllEmployees();
            return Ok(employees);
        }

        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] SaveEmployeeDto request)
        {
            var result = await _employeeManagerService.CreateEmployee(request);
            return Ok(new SaveResponseDto { IsSaved = result });
        }
    }
}