using AtlasERP.Core.Domain.Common;

namespace AtlasERP.Core.Domain.Master;

/// <summary>
/// A single grantable capability, e.g. "employees.view", "payroll.run", "invoices.void".
/// Code is the stable identifier checked in authorization logic; Name/Description are for the UI.
/// </summary>
public class Permission : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
