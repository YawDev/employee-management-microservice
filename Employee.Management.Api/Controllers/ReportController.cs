using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.Api.Controllers
{
    [Authorize(Policy = "ReportUser")] 
    [ApiController]
    [Route("report-line-api")]
    public class ReportController : ControllerBase
    {
        
    }
}