

namespace Employee.Management.Models.Dtos;

public class DepartmentDto
{
    public Guid DepartmentId { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();

    public virtual ICollection<ManagerDto> Managers { get; set; } = new List<ManagerDto>();

    public virtual OrganizationDto Organization { get; set; } = null!;
}
