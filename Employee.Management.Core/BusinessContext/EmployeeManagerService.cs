using AutoMapper;
using Employee.Management.Core.Exceptions;
using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;
using Employee.Management.Models.Dtos.RequestDtos;
using Microsoft.Extensions.Logging;

// 'Employee' is both a namespace root and an entity type — alias the entity to disambiguate.
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Core.BusinessContext
{
    public class EmployeeManagerService(
        IEmployeeRepository employeeRepository,
        IManagerRepository managerRepository,
        IDepartmentRepository departmentRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<EmployeeManagerService> logger) : IEmployeeManagerService
    {
        private const string EmploymentStatusActive = "Active";
        private const string EmploymentStatusInactive = "Inactive";

        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IManagerRepository _managerRepository = managerRepository;
        private readonly IDepartmentRepository _departmentRepository = departmentRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<EmployeeManagerService> _logger = logger;

        #region Employees

        public async Task<bool> CreateEmployee(SaveEmployeeDto employee)
        {
            if (!await _userRepository.DomainUserExistsAsync(employee.DomainUserId))
                throw new NotFoundException("Domain user not found.");

            if (await _departmentRepository.GetDepartmentInfoAsync(employee.DepartmentId) == null)
                throw new NotFoundException("Department not found.");

            if (await _employeeRepository.CheckForExistingDomainUser(employee.DomainUserId))
                throw new BadRequestException("This user already has an employee record.");

            var employeeEntity = new EmployeeEntity
            {
                EmployeeId = Guid.NewGuid(), // PK is app-generated (ValueGeneratedNever)
                DomainUserId = employee.DomainUserId,
                DepartmentId = employee.DepartmentId,
                JobTitle = employee.JobTitle,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                EmploymentStatus = EmploymentStatusActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _employeeRepository.CreateEmployeeAsync(employeeEntity) > 0;
            _logger.LogInformation("Created employee for domain user {DomainUserId} in department {DepartmentId} (success: {Created})",
                employee.DomainUserId, employee.DepartmentId, created);
            return created;
        }

        public async Task<bool> EditEmployee(Guid employeeId, SaveEmployeeDto employee)
        {
            var existingEmployee = await _employeeRepository.GetEmployeeInfoAsync(employeeId);
            if (existingEmployee == null)
                throw new NotFoundException("Employee not found.");

            // An employee record stays bound to its person — re-pointing it would rewrite history.
            if (employee.DomainUserId != existingEmployee.DomainUserId)
                throw new BadRequestException("An employee record cannot be moved to a different user.");

            if (employee.DepartmentId != existingEmployee.DepartmentId
                && await _departmentRepository.GetDepartmentInfoAsync(employee.DepartmentId) == null)
                throw new NotFoundException("Department not found.");

            existingEmployee.DepartmentId = employee.DepartmentId;
            existingEmployee.JobTitle = employee.JobTitle;
            existingEmployee.HireDate = employee.HireDate;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.UpdatedAt = DateTime.UtcNow;

            var updated = await _employeeRepository.EditEmployeeAsync(existingEmployee) > 0;
            _logger.LogInformation("Edited employee {EmployeeId}", employeeId);
            return updated;
        }

        // Soft delete (off-boarding) — employees are never hard-deleted. Flips
        // EmploymentStatus + EndDate; the row stays for history and FK validity.
        // Full off-boarding (DomainUser.IsActive, refresh-token revocation, report
        // reassignment) is a broader flow owned elsewhere.
        public async Task<bool> DeleteEmployee(Guid employeeId)
        {
            var existingEmployee = await _employeeRepository.GetEmployeeInfoAsync(employeeId);
            if (existingEmployee == null)
                throw new NotFoundException("Employee not found.");

            if (existingEmployee.EmploymentStatus == EmploymentStatusInactive)
                throw new BadRequestException("Employee is already off-boarded.");

            existingEmployee.EmploymentStatus = EmploymentStatusInactive;
            existingEmployee.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
            existingEmployee.UpdatedAt = DateTime.UtcNow;

            var deleted = await _employeeRepository.EditEmployeeAsync(existingEmployee) > 0;
            _logger.LogInformation("Off-boarded employee {EmployeeId} (success: {Deleted})", employeeId, deleted);
            return deleted;
        }

        public async Task<List<EmployeeDto>> GetAllEmployees()
        {
            return await _employeeRepository.GetAllEmployeesAsync();
        }

        public async Task<EmployeeDto> GetEmployeeInfo(Guid employeeId)
        {
            var employee = await _employeeRepository.GetEmployeeInfoAsync(employeeId);
            if (employee == null)
                throw new NotFoundException("Employee not found.");

            return _mapper.Map<EmployeeDto>(employee);
        }

        #endregion

        #region Managers

        public async Task<bool> CreateManager(SaveManagerDto manager)
        {
            if (!await _userRepository.DomainUserExistsAsync(manager.DomainUserId))
                throw new NotFoundException("Domain user not found.");

            if (await _departmentRepository.GetDepartmentInfoAsync(manager.DepartmentId) == null)
                throw new NotFoundException("Department not found.");

            // Manager.DomainUserId is unique — one manager designation per person.
            if (await _managerRepository.CheckForExistingDomainUser(manager.DomainUserId))
                throw new BadRequestException("This user is already a manager.");

            var managerEntity = new Manager
            {
                ManagerId = Guid.NewGuid(), // PK is app-generated (ValueGeneratedNever)
                DomainUserId = manager.DomainUserId,
                DepartmentId = manager.DepartmentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _managerRepository.CreateManagerAsync(managerEntity) > 0;
            _logger.LogInformation("Created manager for domain user {DomainUserId} in department {DepartmentId} (success: {Created})",
                manager.DomainUserId, manager.DepartmentId, created);
            return created;
        }

        public async Task<bool> EditManager(Guid managerId, SaveManagerDto manager)
        {
            var existingManager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (existingManager == null)
                throw new NotFoundException("Manager not found.");

            // A manager designation stays bound to its person — changing who it is
            // means deleting this designation and creating a new one.
            if (manager.DomainUserId != existingManager.DomainUserId)
                throw new BadRequestException("A manager record cannot be moved to a different user.");

            if (manager.DepartmentId != existingManager.DepartmentId
                && await _departmentRepository.GetDepartmentInfoAsync(manager.DepartmentId) == null)
                throw new NotFoundException("Department not found.");

            existingManager.DepartmentId = manager.DepartmentId;
            existingManager.UpdatedAt = DateTime.UtcNow;

            var updated = await _managerRepository.EditManagerAsync(existingManager) > 0;
            _logger.LogInformation("Edited manager {ManagerId}", managerId);
            return updated;
        }

        // Removes the manager *designation* row only — the person (DomainUser) is
        // never hard-deleted. Reports must be reassigned first so ReportingLine FKs
        // stay valid.
        public async Task<bool> DeleteManager(Guid managerId)
        {
            var existingManager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (existingManager == null)
                throw new NotFoundException("Manager not found.");

            if (await _managerRepository.HasReportsAsync(managerId))
                throw new BadRequestException("Manager still has reports. Reassign them first.");

            var deleted = await _managerRepository.DeleteManagerAsync(managerId) > 0;
            _logger.LogInformation("Deleted manager {ManagerId} (success: {Deleted})", managerId, deleted);
            return deleted;
        }

        public async Task<List<ManagerDto>> GetAllManagers()
        {
            return await _managerRepository.GetAllManagersAsync();
        }

        public async Task<ManagerDto> GetManagerInfo(Guid managerId)
        {
            var manager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (manager == null)
                throw new NotFoundException("Manager not found.");

            return _mapper.Map<ManagerDto>(manager);
        }

        public async Task<bool> AddReportToManager(Guid managerId, AddReportToManagerDto request)
        {
            var existingManager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (existingManager == null)
                throw new NotFoundException("Manager not found.");

            // request.ReportIds are Employee.EmployeeId values. ReportingLine.ReportId is an
            // FK to DomainUser.DomainUserId, so each employee must be resolved to its
            // DomainUserId before being written — the two GUIDs are not interchangeable.
            var domainUserIds = new List<Guid>();
            foreach (var reportId in request.ReportIds)
            {
                var reportEmployee = await _employeeRepository.GetEmployeeInfoAsync(reportId);
                if (reportEmployee == null)
                    throw new NotFoundException($"Report employee with ID {reportId} not found.");

                domainUserIds.Add(reportEmployee.DomainUserId);
            }

            var added = await _managerRepository.AddReports(managerId, domainUserIds);
            _logger.LogInformation("Added reports to manager {ManagerId} (success: {Added})", managerId, added);
            return added;
        }

        #endregion
    }
}
