using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.Api.Controllers
{
    [Authorize(Policy = "CompanyAdmin")] 
    [ApiController]
    [Route("company-admin-api")]
    public class AdminController : ControllerBase
    {
        
    }
}