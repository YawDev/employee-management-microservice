namespace Employee.Management.Models.Dtos
{
    public class TenantDto
    {
        public int TenantId { get; set; }
        public Guid Uid { get; set; }
        public string Name { get; set; } = null!;
        public string? Logo { get; set; }

    }
}
