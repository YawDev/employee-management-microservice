namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class ReportLineResponseDto
    {
        public Guid ReportId { get; set; }
        public Guid ManagerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

    }


}
