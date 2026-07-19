using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.Api.Controllers
{
    [Authorize(Policy = "CompanyPermission")] 
    [ApiController]
    [Route("company-admin-api")]
    public class CompanyController : ControllerBase
    {
        
    }
}