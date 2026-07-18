namespace AtlasERP.Core.Domain.Master;

/// <summary>
/// Grants a user access to a specific company's database. A user with no entries here
/// cannot open any company; a user with entries for all four brands can switch between them.
/// </summary>
public class UserCompanyAccess
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public DateTime GrantedAtUtc { get; set; } = DateTime.UtcNow;
}
