using AtlasERP.Core.Domain.Common;

namespace AtlasERP.Core.Domain.Master;

/// <summary>
/// A registry entry in the Master DB describing one branded company and where its own
/// per-company database lives. The Presentation layer never talks to a company's data
/// without first resolving one of these.
/// </summary>
public class Company : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }

    /// <summary>Hex color (e.g. "#2CA01C") derived from the uploaded logo, or set manually. Used to brand printed documents.</summary>
    public string? BrandColorHex { get; set; }

    /// <summary>
    /// Connection string to this company's dedicated database. Stored encrypted at rest
    /// (see Infrastructure connection-string protection); never logged in plain text.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    public string? InvoiceTemplateRef { get; set; }
    public string? PayslipTemplateRef { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserCompanyAccess> UserAccessEntries { get; set; } = new List<UserCompanyAccess>();
}
