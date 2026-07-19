namespace Employee.Management.Models.Dtos.RequestDtos
{
    public class SaveDepartmentDto
    {
        public int OrganizationId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
}
