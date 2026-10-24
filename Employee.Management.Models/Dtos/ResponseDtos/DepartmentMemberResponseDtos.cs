namespace Employee.Management.Models.Dtos.ResponseDtos
{
    // Short employee view for get-department-info. EmployeeId is what get-employee-info takes.
    public class DepartmentEmployeeResponseDto
    {
        public Guid EmployeeId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? JobTitle { get; set; }
    }

    // Short manager view for get-department-info. ManagerId is what get-manager-info takes.
    public class DepartmentManagerResponseDto
    {
        public Guid ManagerId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;
    }
}
