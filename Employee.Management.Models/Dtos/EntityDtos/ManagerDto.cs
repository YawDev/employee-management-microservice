

namespace Employee.Management.Models.Dtos;

public class ManagerDto
{
    public Guid ManagerId { get; set; }

    public Guid DomainUserId { get; set; }

    public Guid DepartmentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual DepartmentDto Department { get; set; } = null!;

    public virtual DomainUserDto DomainUser { get; set; } = null!;

    public virtual ICollection<ReportingLineDto> ReportingLines { get; set; } = new List<ReportingLineDto>();


}
