namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class ManagerInfoResponseDto
    {
        public Guid ManagerId { get; set; }

        public Guid DomainUserId { get; set; }

        public Guid DepartmentId { get; set; }

        public virtual string Department { get; set; } = null!;

        public virtual DomainUserResponseDto DomainUser { get; set; } = null!;

        public virtual ICollection<ReportLineResponseDto> ReportingLines { get; set; } = new List<ReportLineResponseDto>();
    }

}
