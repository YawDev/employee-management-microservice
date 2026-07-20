namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class OrganizationResponseDto
    {
        public int OrganizationId { get; set; }

        public Guid Uid { get; set; }

        public int TenantId { get; set; }

        public string Name { get; set; } = null!;

        public string? Industry { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<DepartmentResponseDto> Departments { get; set; } = new List<DepartmentResponseDto>();

        public virtual TenantDto Tenant { get; set; } = null!;
    }


}
