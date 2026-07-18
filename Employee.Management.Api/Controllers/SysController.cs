using Employee.Management.Core.Interfaces.Business;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.Api.Controllers
{
    
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
    }
}