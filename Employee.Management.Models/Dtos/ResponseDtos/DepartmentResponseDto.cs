namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class DepartmentResponseDto
    {
        public Guid DepartmentId { get; set; }

        public int OrganizationId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public virtual ICollection<EmployeeResponseDto> Employees { get; set; } = new List<EmployeeResponseDto>();

        public virtual ICollection<ManagerInfoResponseDto> Managers { get; set; } = new List<ManagerInfoResponseDto>();

    }

}
