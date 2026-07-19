namespace Employee.Management.Models.Dtos.RequestDtos
{
    public class SaveReportingLineDto
    {
        // The subordinate — an IC's or a manager's DomainUser.
        public Guid ReportId { get; set; }

        // The boss — always a designated Manager.
        public Guid ManagerId { get; set; }
    }
}
