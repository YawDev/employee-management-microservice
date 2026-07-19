namespace Employee.Management.Models.Dtos.RequestDtos
{
    public class SaveOrganizationDto
    {
        public int TenantId { get; set; }

        public string Name { get; set; } = null!;

        public string? Industry { get; set; }
    }
}
