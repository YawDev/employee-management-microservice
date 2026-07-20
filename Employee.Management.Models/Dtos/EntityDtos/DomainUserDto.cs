using Microsoft.AspNetCore.Identity;

namespace Employee.Management.Models.Dtos
{
    // DTO For Domain User 
    public class DomainUserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid IdentityUserId { get; set; }
        public TenantDto Tenant { get; set; }
        public bool IsActive { get; set; }
        public string? JobTitle { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? SupervisorId { get; set; }
    }
}
