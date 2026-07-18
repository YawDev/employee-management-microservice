using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Employee.Management.Api.Controllers
{
    //TODO: Add protections to prevent unauthorized access to sys endpoints
    [ApiController]
    [Route("sys-api")]
    public class SysController(ITenantService tenantService) : ControllerBase
    {
        private readonly ITenantService _tenantService = tenantService;

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

        [HttpPost("edit-tenant/{id}")]
        public async Task<IActionResult> EditTenant(int id, [FromBody] CreateTenantDto createTenantDto)
        {

            //var isSuccess = await _tenantService.EditTenant(id, createTenantDto);
            return Ok(new SaveTenantDto { IsSaved = true });
        }
        
        [HttpPost("delete-tenant/{id}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var isSuccess = await _tenantService.DeleteTenant(id);
            return Ok(new DeleteTenantDto { IsDeleted = isSuccess });
        }
    }
}