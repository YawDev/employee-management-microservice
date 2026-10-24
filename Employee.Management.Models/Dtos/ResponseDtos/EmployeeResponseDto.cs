namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class EmployeeResponseDto
    {
        public Guid EmployeeId { get; set; }

        // Set when this employee is also a manager, so callers can send them to the manager view.
        public Guid? ManagerId { get; set; }

        public Guid DomainUserId { get; set; }

        public Guid DepartmentId { get; set; }

        public string? JobTitle { get; set; }

        public DateOnly? HireDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public decimal? Salary { get; set; }

        public string EmploymentStatus { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual string Department { get; set; } = null!;

        // The department's organization, so callers can build the ownership trail without another lookup.
        public int OrganizationId { get; set; }

        public virtual DomainUserResponseDto DomainUser { get; set; } = null!;

    }


}
