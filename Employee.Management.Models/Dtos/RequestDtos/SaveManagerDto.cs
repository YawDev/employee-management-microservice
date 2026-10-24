using System;

namespace Employee.Management.Models.Dtos.RequestDtos
{
    // A manager is an existing employee: their person and department come from that Employee row.
    public class SaveManagerDto
    {
        public Guid EmployeeId { get; set; }
    }
}
