namespace Employee.Management.Models.Dtos.RequestDtos
{
    public class SaveManagerDto
    {
        public Guid DomainUserId { get; set; }

        public Guid DepartmentId { get; set; }
    }
}
