using AtlasERP.Core.Domain.Common;

namespace AtlasERP.Core.Domain.Sales;

public class Customer : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxId { get; set; }
    public string? Notes { get; set; }
}
