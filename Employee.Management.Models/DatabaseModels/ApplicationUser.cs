using System;
using Microsoft.AspNetCore.Identity;
namespace Employee.Management.Models.DatabaseModels;

/// <summary>
/// Represents an application user in the identity system, extending the IdentityUser class with a Guid as the primary key type.
/// </summary>
public partial class ApplicationUser : IdentityUser<Guid>
{
    // You can add custom properties here if needed
}
