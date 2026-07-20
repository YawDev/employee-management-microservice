using AutoMapper;
using Employee.Management.Core.Exceptions;
using Employee.Management.Core.Interfaces.Business;
using Employee.Management.Core.Interfaces.Repositories;
using Employee.Management.Models.DatabaseModels;
using Employee.Management.Models.Dtos;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;
using Microsoft.Extensions.Logging;

namespace Employee.Management.Core.BusinessContext
{
    public class CompanyService(
        IOrganizationRepository organizationRepository,
        IDepartmentRepository departmentRepository,
        ITenantRepository tenantRepository,
        IMapper mapper,
        ILogger<CompanyService> logger) : ICompanyService
    {
        private readonly IOrganizationRepository _organizationRepository = organizationRepository;
        private readonly IDepartmentRepository _departmentRepository = departmentRepository;
        private readonly ITenantRepository _tenantRepository = tenantRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CompanyService> _logger = logger;

        #region Organizations

        public async Task<bool> CreateOrganization(SaveOrganizationDto organization)
        {
            var tenant = await _tenantRepository.GetTenantInfoAsync(organization.TenantId);
            if (tenant == null)
                throw new NotFoundException("Tenant not found.");

            if (await _organizationRepository.CheckForExistingName(organization.Name, organization.TenantId))
                throw new BadRequestException("Organization name already exists for this tenant.");

            var organizationEntity = new Organization
            {
                Uid = Guid.NewGuid(), // Uid is app-generated (column has no EF-known default)
                TenantId = organization.TenantId,
                Name = organization.Name,
                Industry = organization.Industry,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _organizationRepository.CreateOrganizationAsync(organizationEntity) > 0;
            _logger.LogInformation("Created organization {OrganizationName} in tenant {TenantId} (success: {Created})",
                organization.Name, organization.TenantId, created);
            return created;
        }

        public async Task<bool> EditOrganization(int organizationId, SaveOrganizationDto organization)
        {
            var existingOrganization = await _organizationRepository.GetOrganizationInfoAsync(organizationId);
            if (existingOrganization == null)
                throw new NotFoundException("Organization not found.");

            if (organization.TenantId != existingOrganization.TenantId
                && await _tenantRepository.GetTenantInfoAsync(organization.TenantId) == null)
                throw new NotFoundException("Tenant not found.");

            if (await _organizationRepository.CheckForExistingName(organization.Name, organization.TenantId, organizationId))
                throw new BadRequestException("Organization name already exists for this tenant.");

            existingOrganization.TenantId = organization.TenantId;
            existingOrganization.Name = organization.Name;
            existingOrganization.Industry = organization.Industry;
            existingOrganization.UpdatedAt = DateTime.UtcNow;

            var updated = await _organizationRepository.EditOrganizationAsync(existingOrganization) > 0;
            _logger.LogInformation("Edited organization {OrganizationId}", organizationId);
            return updated;
        }

        public async Task<bool> DeleteOrganization(int organizationId)
        {
            var existingOrganization = await _organizationRepository.GetOrganizationInfoAsync(organizationId);
            if (existingOrganization == null)
                throw new NotFoundException("Organization not found.");

            if (await _organizationRepository.HasDepartmentsAsync(organizationId))
                throw new BadRequestException("Organization still has departments. Delete or move them first.");

            var deleted = await _organizationRepository.DeleteOrganizationAsync(organizationId) > 0;
            _logger.LogInformation("Deleted organization {OrganizationId} (success: {Deleted})", organizationId, deleted);
            return deleted;
        }

        public async Task<List<OrganizationResponseDto>> GetAllOrganizations()
        {
            return await _organizationRepository.GetAllOrganizationsAsync();
        }

        public async Task<OrganizationResponseDto> GetOrganizationInfo(int organizationId)
        {
            var organization = await _organizationRepository.GetOrganizationInfoAsync(organizationId);
            if (organization == null)
                throw new NotFoundException("Organization not found.");

            return _mapper.Map<OrganizationResponseDto>(organization);
        }

        #endregion

        #region Departments

        public async Task<bool> CreateDepartment(SaveDepartmentDto department)
        {
            var organization = await _organizationRepository.GetOrganizationInfoAsync(department.OrganizationId);
            if (organization == null)
                throw new NotFoundException("Organization not found.");

            if (await _departmentRepository.CheckForExistingName(department.Name, department.OrganizationId))
                throw new BadRequestException("Department name already exists for this organization.");

            var departmentEntity = new Department
            {
                DepartmentId = Guid.NewGuid(), // PK is app-generated (ValueGeneratedNever)
                OrganizationId = department.OrganizationId,
                Name = department.Name,
                Description = department.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _departmentRepository.CreateDepartmentAsync(departmentEntity) > 0;
            _logger.LogInformation("Created department {DepartmentName} in organization {OrganizationId} (success: {Created})",
                department.Name, department.OrganizationId, created);
            return created;
        }

        public async Task<bool> EditDepartment(Guid departmentId, SaveDepartmentDto department)
        {
            var existingDepartment = await _departmentRepository.GetDepartmentInfoAsync(departmentId);
            if (existingDepartment == null)
                throw new NotFoundException("Department not found.");

            if (department.OrganizationId != existingDepartment.OrganizationId
                && await _organizationRepository.GetOrganizationInfoAsync(department.OrganizationId) == null)
                throw new NotFoundException("Organization not found.");

            if (await _departmentRepository.CheckForExistingName(department.Name, department.OrganizationId, departmentId))
                throw new BadRequestException("Department name already exists for this organization.");

            existingDepartment.OrganizationId = department.OrganizationId;
            existingDepartment.Name = department.Name;
            existingDepartment.Description = department.Description;
            existingDepartment.UpdatedAt = DateTime.UtcNow;

            var updated = await _departmentRepository.EditDepartmentAsync(existingDepartment) > 0;
            _logger.LogInformation("Edited department {DepartmentId}", departmentId);
            return updated;
        }

        public async Task<bool> DeleteDepartment(Guid departmentId)
        {
            var existingDepartment = await _departmentRepository.GetDepartmentInfoAsync(departmentId);
            if (existingDepartment == null)
                throw new NotFoundException("Department not found.");

            if (await _departmentRepository.HasMembersAsync(departmentId))
                throw new BadRequestException("Department still has employees or managers. Reassign them first.");

            var deleted = await _departmentRepository.DeleteDepartmentAsync(departmentId) > 0;
            _logger.LogInformation("Deleted department {DepartmentId} (success: {Deleted})", departmentId, deleted);
            return deleted;
        }

        public async Task<List<DepartmentDto>> GetAllDepartments()
        {
            return await _departmentRepository.GetAllDepartmentsAsync();
        }

        public async Task<DepartmentDto> GetDepartmentInfo(Guid departmentId)
        {
            var department = await _departmentRepository.GetDepartmentInfoAsync(departmentId);
            if (department == null)
                throw new NotFoundException("Department not found.");

            return _mapper.Map<DepartmentDto>(department);
        }

        #endregion
    }
}
