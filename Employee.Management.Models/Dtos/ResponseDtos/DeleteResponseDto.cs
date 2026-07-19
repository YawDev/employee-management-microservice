namespace Employee.Management.Models.Dtos.ResponseDtos
{
    // Shared delete result for any entity — mirrors DeleteTenantDto's shape.
    public class DeleteResponseDto
    {
        public bool IsDeleted { get; set; }
    }
}
