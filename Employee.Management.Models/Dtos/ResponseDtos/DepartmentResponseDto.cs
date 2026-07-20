namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class DepartmentResponseDto
    {
        public Guid DepartmentId { get; set; }

        public int OrganizationId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<EmployeeResponseDto> Employees { get; set; } = new List<EmployeeResponseDto>();

        public virtual ICollection<ManagerResponseDto> Managers { get; set; } = new List<ManagerResponseDto>();

    }

    public class ManagerResponseDto
    {
        public Guid ManagerId { get; set; }

        public Guid DomainUserId { get; set; }

        public Guid DepartmentId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual DepartmentResponseDto Department { get; set; } = null!;

        public virtual DomainUserResponseDto DomainUser { get; set; } = null!;
    }

}
