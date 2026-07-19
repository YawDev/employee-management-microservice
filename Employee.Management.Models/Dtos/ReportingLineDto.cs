

namespace Employee.Management.Models.Dtos;

public class ReportingLineDto
{
    public Guid ReportId { get; set; }

    public Guid ManagerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ManagerDto Manager { get; set; } = null!;

    public virtual DomainUserDto Report { get; set; } = null!;

}