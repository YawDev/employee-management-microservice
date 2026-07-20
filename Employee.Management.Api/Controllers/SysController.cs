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
    public class SysController(ITenantService tenantService, ICompanyService companyService) : ControllerBase
    {
        private readonly ITenantService _tenantService = tenantService;
        private readonly ICompanyService _companyService = companyService;

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
    }
}