using System;
using System.Collections.Generic;

namespace Employee.Management.Models.DatabaseModels;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = null!;

    public Guid IdentityUserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }
}
