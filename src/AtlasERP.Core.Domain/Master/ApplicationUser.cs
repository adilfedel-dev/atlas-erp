using AtlasERP.Core.Domain.Common;

namespace AtlasERP.Core.Domain.Master;

/// <summary>
/// A login identity in the Master DB. Scoped to one or more companies via
/// <see cref="UserCompanyAccess"/> — a single account can span all four brands or just one.
/// </summary>
public class ApplicationUser : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserCompanyAccess> CompanyAccessEntries { get; set; } = new List<UserCompanyAccess>();
}
