namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class ManagerInfoResponseDto
    {
        public Guid ManagerId { get; set; }

        // The manager's own HR record (a manager is an employee).
        public Guid EmployeeId { get; set; }

        public string? JobTitle { get; set; }

        public DateOnly? HireDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public decimal? Salary { get; set; }

        public string EmploymentStatus { get; set; } = null!;

        public Guid DomainUserId { get; set; }

        public Guid DepartmentId { get; set; }

        public virtual string Department { get; set; } = null!;

        // The department's organization, so callers can build the ownership trail without another lookup.
        public int OrganizationId { get; set; }

        public virtual DomainUserResponseDto DomainUser { get; set; } = null!;

        public virtual ICollection<ReportLineResponseDto> ReportingLines { get; set; } = new List<ReportLineResponseDto>();
    }

}
