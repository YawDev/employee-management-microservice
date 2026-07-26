using AutoMapper;
using Employee.Management.Core.Exceptions;
using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;
using Microsoft.Extensions.Logging;

// 'Employee' is both a namespace root and an entity type — alias the entity to disambiguate.
using EmployeeEntity = Employee.Management.Models.DatabaseModels.Employee;

namespace Employee.Management.Core.BusinessContext
{
    public class EmployeeManagerService(
        IEmployeeRepository employeeRepository,
        IManagerRepository managerRepository,
        IDepartmentRepository departmentRepository,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<EmployeeManagerService> logger) : IEmployeeManagerService
    {
        private const string EmploymentStatusActive = "Active";
        private const string EmploymentStatusInactive = "Inactive";

        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IManagerRepository _managerRepository = managerRepository;
        private readonly IDepartmentRepository _departmentRepository = departmentRepository;
        private readonly IOrganizationRepository _organizationRepository = organizationRepository;
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

        public async Task<List<EmployeeResponseDto>> GetAllEmployees()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return _mapper.Map<List<EmployeeResponseDto>>(employees);
        }

        public async Task<EmployeeResponseDto> GetEmployeeInfo(Guid employeeId)
        {
            var employee = await _employeeRepository.GetEmployeeInfoAsync(employeeId);
            if (employee == null)
                throw new NotFoundException("Employee not found.");

            return _mapper.Map<EmployeeResponseDto>(employee);
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

        public async Task<List<ManagerInfoResponseDto>> GetAllManagers()
        {
            var managers = await _managerRepository.GetAllManagersAsync();
            return _mapper.Map<List<ManagerInfoResponseDto>>(managers);
        }

        public async Task<ManagerInfoResponseDto> GetManagerInfo(Guid managerId)
        {
            var manager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (manager == null)
                throw new NotFoundException("Manager not found.");

            return _mapper.Map<ManagerInfoResponseDto>(manager);
        }

        public async Task<bool> AddReportToManager(Guid managerId, AddReportToManagerDto request)
        {
            var existingManager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (existingManager == null)
                throw new NotFoundException("Manager not found.");

            await EnsureReportsExist(request.ReportIds);
            await EnsureReportsUnassigned(request.ReportIds);

            var added = await _managerRepository.AddReports(managerId, request.ReportIds);
            _logger.LogInformation("Added {Count} report(s) to manager {ManagerId} (success: {Added})",
                request.ReportIds.Count, managerId, added);
            return added;
        }

        // System-level: unassign a single report from a manager.
        public async Task<bool> RemoveReportFromManager(Guid managerId, Guid reportId)
        {
            var existingManager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (existingManager == null)
                throw new NotFoundException("Manager not found.");

            var removed = await _managerRepository.RemoveReports(managerId, new List<Guid> { reportId });
            _logger.LogInformation("Removed report {ReportId} from manager {ManagerId} (success: {Removed})",
                reportId, managerId, removed);
            return removed;
        }

        #endregion

        #region Organization-scoped (Company Admin)

        public async Task<List<ManagerInfoResponseDto>> GetAllManagersForOrganization(int OrganizationId)
        {
            await EnsureOrganizationExists(OrganizationId);
            var managers = await _managerRepository.GetAllManagersForOrganizationAsync(OrganizationId);
            return _mapper.Map<List<ManagerInfoResponseDto>>(managers);
        }

        public async Task<ManagerInfoResponseDto> GetManagerInfoForOrganization(Guid managerId, int OrganizationId)
        {
            var manager = await GetManagerInOrganization(managerId, OrganizationId);
            return _mapper.Map<ManagerInfoResponseDto>(manager);
        }

        public async Task<bool> CreateManagerForOrganization(SaveManagerDto manager, int OrganizationId)
        {
            await EnsureDepartmentInOrganization(manager.DepartmentId, OrganizationId);
            return await CreateManager(manager);
        }

        public async Task<bool> EditManagerForOrganization(Guid managerId, SaveManagerDto manager, int OrganizationId)
        {
            await GetManagerInOrganization(managerId, OrganizationId);
            await EnsureDepartmentInOrganization(manager.DepartmentId, OrganizationId);
            return await EditManager(managerId, manager);
        }

        public async Task<List<EmployeeResponseDto>> GetAllEmployeesForOrganization(int OrganizationId)
        {
            await EnsureOrganizationExists(OrganizationId);
            var employees = await _employeeRepository.GetAllEmployeesForOrganizationAsync(OrganizationId);
            return _mapper.Map<List<EmployeeResponseDto>>(employees);
        }

        public async Task<EmployeeResponseDto> GetEmployeeInfoForOrganization(Guid employeeId, int OrganizationId)
        {
            var employee = await _employeeRepository.GetEmployeeInfoForOrganizationAsync(OrganizationId, employeeId);
            if (employee == null)
                throw new NotFoundException("Employee not found in this organization.");

            return _mapper.Map<EmployeeResponseDto>(employee);
        }

        public async Task<bool> CreateEmployeeForOrganization(SaveEmployeeDto employee, int OrganizationId)
        {
            await EnsureDepartmentInOrganization(employee.DepartmentId, OrganizationId);
            return await CreateEmployee(employee);
        }

        public async Task<bool> EditEmployeeForOrganization(Guid employeeId, SaveEmployeeDto employee, int OrganizationId)
        {
            var existingEmployee = await _employeeRepository.GetEmployeeInfoForOrganizationAsync(OrganizationId, employeeId);
            if (existingEmployee == null)
                throw new NotFoundException("Employee not found in this organization.");

            // An employee record stays bound to its person — re-pointing it would rewrite history.
            if (employee.DomainUserId != existingEmployee.DomainUserId)
                throw new BadRequestException("An employee record cannot be moved to a different user.");

            await EnsureDepartmentInOrganization(employee.DepartmentId, OrganizationId);

            existingEmployee.DepartmentId = employee.DepartmentId;
            existingEmployee.JobTitle = employee.JobTitle;
            existingEmployee.HireDate = employee.HireDate;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.UpdatedAt = DateTime.UtcNow;

            var updated = await _employeeRepository.EditEmployeeForOrganizationAsync(OrganizationId, existingEmployee) > 0;
            _logger.LogInformation("Edited employee {EmployeeId} in organization {OrganizationId}", employeeId, OrganizationId);
            return updated;
        }

        public async Task<List<ReportLineResponseDto>> GetAllReportsForManager(Guid managerId, int OrganizationId)
        {
            await GetManagerInOrganization(managerId, OrganizationId);
            var reports = await _managerRepository.GetAllReportsForManager(managerId);
            return _mapper.Map<List<ReportLineResponseDto>>(reports);
        }

        public async Task<ReportLineResponseDto> GetReportInfoForManager(Guid managerId, Guid reportId, int OrganizationId)
        {
            await GetManagerInOrganization(managerId, OrganizationId);
            var report = await _managerRepository.GetReportInfoForManager(managerId, reportId);
            if (report == null)
                throw new NotFoundException("Report not found for this manager.");

            return _mapper.Map<ReportLineResponseDto>(report);
        }

        public Task<bool> AddReportToManager(Guid managerId, List<Guid> reportIds, int OrganizationId)
        {
            return AddReportToManagerForOrganization(managerId, reportIds, OrganizationId);
        }

        public Task<bool> RemoveReportFromManager(Guid managerId, List<Guid> reportIds, int OrganizationId)
        {
            return RemoveReportFromManagerForOrganization(managerId, reportIds, OrganizationId);
        }

        public async Task<bool> AddReportToManagerForOrganization(Guid managerId, List<Guid> reportIds, int OrganizationId)
        {
            await GetManagerInOrganization(managerId, OrganizationId);
            await EnsureReportsExist(reportIds);
            await EnsureReportsUnassigned(reportIds);

            var added = await _managerRepository.AddReports(managerId, reportIds);
            _logger.LogInformation("Added {Count} report(s) to manager {ManagerId} in organization {OrganizationId} (success: {Added})",
                reportIds.Count, managerId, OrganizationId, added);
            return added;
        }

        public async Task<bool> RemoveReportFromManagerForOrganization(Guid managerId, List<Guid> reportIds, int OrganizationId)
        {
            await GetManagerInOrganization(managerId, OrganizationId);

            var removed = await _managerRepository.RemoveReports(managerId, reportIds);
            _logger.LogInformation("Removed {Count} report(s) from manager {ManagerId} in organization {OrganizationId} (success: {Removed})",
                reportIds.Count, managerId, OrganizationId, removed);
            return removed;
        }

        #endregion

        #region Helpers

        private async Task EnsureOrganizationExists(int organizationId)
        {
            if (await _organizationRepository.GetOrganizationInfoAsync(organizationId) == null)
                throw new NotFoundException("Organization not found.");
        }

        // Report ids are DomainUserIds (they are written straight to ReportingLine.ReportId,
        // its FK to DomainUser). Validate each resolves to a real person before adding.
        private async Task EnsureReportsExist(List<Guid> domainUserIds)
        {
            var existingDomainUsers = await _userRepository.AllDomainUsersExistAsync(domainUserIds);

            var IdsNotFound = domainUserIds.Where(u => !existingDomainUsers.Any(e => e.DomainUserId == u)).ToList();
            if (IdsNotFound.Any())
                throw new NotFoundException($"These report domain user IDs were not found: {string.Join(", ", IdsNotFound)}");

        }

        // ReportingLine.ReportId is the PK — one manager per person. Adding a report that already
        // reports to a manager would hit a PK violation, so reject it as a 400 up front.
        private async Task EnsureReportsUnassigned(List<Guid> domainUserIds)
        {
            var alreadyAssigned = await _managerRepository.GetAssignedReportIds(domainUserIds);
            if (alreadyAssigned.Count > 0)
                throw new BadRequestException(
                    $"These reports already report to a manager: {string.Join(", ", alreadyAssigned)}. Remove them first.");
        }

        private async Task<Manager> GetManagerInOrganization(Guid managerId, int organizationId)
        {
            await EnsureOrganizationExists(organizationId);

            var manager = await _managerRepository.GetManagerInfoAsync(managerId);
            if (manager == null)
                throw new NotFoundException("Manager not found.");
            if (manager.Department.OrganizationId != organizationId)
                throw new BadRequestException("Manager does not belong to the specified organization.");

            return manager;
        }

        private async Task EnsureDepartmentInOrganization(Guid departmentId, int organizationId)
        {
            await EnsureOrganizationExists(organizationId);

            var department = await _departmentRepository.GetDepartmentInfoAsync(departmentId);
            if (department == null)
                throw new NotFoundException("Department not found.");

            if (department.OrganizationId != organizationId)
                throw new BadRequestException("Department does not belong to the specified organization.");
        }

        #endregion
    }
}
