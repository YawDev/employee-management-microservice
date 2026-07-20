using Employee.Management.Models.Dtos;
using Employee.Management.Models.Dtos.RequestDtos;
using Employee.Management.Models.Dtos.ResponseDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee.Management.Core.Interfaces.Business
{
    // Company structure: Organization + Department operations.
    public interface ICompanyService
    {
        public Task<List<OrganizationResponseDto>> GetAllOrganizations();
        public Task<OrganizationResponseDto> GetOrganizationInfo(int organizationId);
        public Task<bool> CreateOrganization(SaveOrganizationDto organization);
        public Task<bool> EditOrganization(int organizationId, SaveOrganizationDto organization);
        public Task<bool> DeleteOrganization(int organizationId);

        public Task<List<DepartmentDto>> GetAllDepartments();
        public Task<DepartmentDto> GetDepartmentInfo(Guid departmentId);
        public Task<bool> CreateDepartment(SaveDepartmentDto department);
        public Task<bool> EditDepartment(Guid departmentId, SaveDepartmentDto department);
        public Task<bool> DeleteDepartment(Guid departmentId);
    }
}
