namespace Employee.Management.Models.Dtos.RequestDtos
{
    public class CreateTenantDto
    {
        public string Name { get; set; } = null!;
        public string? Logo { get; set; }
    }
}