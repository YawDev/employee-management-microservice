using Employee.Management.Core.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;
using AutoMapper;   

namespace Employee.Management.Api.Controllers
{
    [ApiController]
    [Route("report-line-api")]
    public class ReportLineController(IEmployeeManagerService employeeManagerService, IMapper mapper) : ControllerBase
    {
        private readonly IEmployeeManagerService _employeeManagerService = employeeManagerService;
        private readonly IMapper _mapper = mapper;

        [Authorize(Policy = "EmployeePermission")]
        [HttpGet("get-employee-info/{employeeId}")]
        public async Task<IActionResult> GetEmployeeInfo(Guid employeeId)
        {
            var employeeInfo = await _employeeManagerService.GetEmployeeInfo(employeeId);
            return Ok(employeeInfo);
        }

        [Authorize(Policy = "ManagerPermission")]
        [HttpGet("get-manager-info/{managerId}")]
        public async Task<IActionResult> GetManagerInfo(Guid managerId)
        {
            var managerInfo = await _employeeManagerService.GetManagerInfo(managerId);
            var response = _mapper.Map<ManagerInfoResponseDto>(managerInfo);
            return Ok(response);
        }

    }
}