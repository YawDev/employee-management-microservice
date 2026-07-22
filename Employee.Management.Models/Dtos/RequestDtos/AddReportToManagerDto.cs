using System.Collections.Generic;

namespace Employee.Management.Models.Dtos.RequestDtos
{
    public class AddReportToManagerDto
    {
        // DomainUserIds of the reports to be added to the manager
        public List<Guid> ReportIds { get; set; } = new List<Guid>();

        public Guid ManagerId { get; set; }
    }
}
