namespace Employee.Management.Models.Dtos.ResponseDtos
{
    public class GetAllOrganizationResponseDto
    {
        public ICollection<OrganizationResponseDto> Organizations { get; set; } = new List<OrganizationResponseDto>();
    }

}
