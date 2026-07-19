namespace Employee.Management.Models.Dtos.RequestDtos
{
    public class SaveEmployeeDto
    {
        public Guid DomainUserId { get; set; }

        public Guid DepartmentId { get; set; }

        public string? JobTitle { get; set; }

        public DateOnly? HireDate { get; set; }

        public decimal? Salary { get; set; }
    }
}