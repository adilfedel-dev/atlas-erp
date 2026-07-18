using AtlasERP.Core.Domain.Common;

namespace AtlasERP.Core.Domain.Master;

/// <summary>
/// Cross-company audit trail kept in the Master DB (login, company switch, user/role
/// management, etc.). Per-company business-data audit trails live in each company DB instead.
/// </summary>
public class AuditLogEntry : EntityBase
{
    public Guid? UserId { get; set; }
    public Guid? CompanyId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? DetailsJson { get; set; }
}
