namespace Employee.Management.Models.Dtos.ResponseDtos
{
    // get-department-info only: the department with a short view of its people.
    // DepartmentResponseDto stays as-is for the list and organization endpoints.
    public class DepartmentDetailsResponseDto
    {
        public Guid DepartmentId { get; set; }

        public int OrganizationId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public ICollection<DepartmentManagerResponseDto> Managers { get; set; } = new List<DepartmentManagerResponseDto>();

        public ICollection<DepartmentEmployeeResponseDto> Employees { get; set; } = new List<DepartmentEmployeeResponseDto>();
    }
}
