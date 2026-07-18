using AtlasERP.Core.Domain.Payroll.Enums;

namespace AtlasERP.Core.Domain.Payroll;

public class PayslipLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PayslipId { get; set; }
    public Payslip? Payslip { get; set; }

    public PayslipLineItemType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Always stored positive; whether it adds to or subtracts from net pay is determined by <see cref="Type"/>.</summary>
    public decimal Amount { get; set; }
}
