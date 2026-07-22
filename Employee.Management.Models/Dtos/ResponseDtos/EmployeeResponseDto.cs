namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class EmployeeResponseDto
    {
        public Guid EmployeeId { get; set; }

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

    }


}
