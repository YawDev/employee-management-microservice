using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee.Management.Core.Interfaces.Business
{
    // Company structure: Organization + Department operations.
    public interface ICompanyService
    {
        #region System Level Operations
        public Task<List<OrganizationResponseDto>> GetAllOrganizations();
        public Task<OrganizationResponseDto> GetOrganizationInfo(int organizationId);
        public Task<bool> CreateOrganization(SaveOrganizationDto organization);
        public Task<bool> EditOrganization(int organizationId, SaveOrganizationDto organization);
        public Task<bool> DeleteOrganization(int organizationId);
        public Task<List<DepartmentResponseDto>> GetAllDepartments();
        public Task<DepartmentResponseDto> GetDepartmentInfo(Guid departmentId);
        public Task<bool> CreateDepartment(SaveDepartmentDto department);
        public Task<bool> EditDepartment(Guid departmentId, SaveDepartmentDto department);
        public Task<bool> DeleteDepartment(Guid departmentId);
        #endregion

        #region Organization Level Operations
        public Task<List<DepartmentResponseDto>> GetAllDepartmentsForOrganization(int organizationId);
        public Task<DepartmentResponseDto> GetDepartmentInfoForOrganization(int organizationId, Guid departmentId);
        public Task<bool> CreateDepartmentForOrganization(int organizationId, SaveDepartmentDto department);
        public Task<bool> EditDepartmentForOrganization(int organizationId, Guid departmentId, SaveDepartmentDto department);

        #endregion
    }
}
