namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class DomainUserResponseDto
    {
        public Guid DomainUserId { get; set; }

        public int TenantId { get; set; }

        public string TenantName { get; set; } = null!;

        public Guid IdentityUserId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

    }

}
