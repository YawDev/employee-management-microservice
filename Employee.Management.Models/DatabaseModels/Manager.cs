using System;
using System.Collections.Generic;

namespace Employee.Management.Models.DatabaseModels;

public partial class Manager
{
    public Guid ManagerId { get; set; }

    // The manager's HR record. DomainUserId/DepartmentId below are kept in step with it by the
    // DB (composite FK_Manager_Employee, ON UPDATE CASCADE).
    public Guid EmployeeId { get; set; }

    public Guid DomainUserId { get; set; }

    public Guid DepartmentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual DomainUser DomainUser { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<ReportingLine> ReportingLines { get; set; } = new List<ReportingLine>();
}
