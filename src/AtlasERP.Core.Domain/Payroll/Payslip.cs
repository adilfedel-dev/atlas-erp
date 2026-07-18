using AtlasERP.Core.Domain.Common;
using AtlasERP.Core.Domain.HumanResources;

namespace AtlasERP.Core.Domain.Payroll;

public class Payslip : EntityBase
{
    public Guid PayrollRunId { get; set; }
    public PayrollRun? PayrollRun { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public decimal GrossPay { get; set; }
    public decimal TotalBonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }

    public ICollection<PayslipLineItem> LineItems { get; set; } = new List<PayslipLineItem>();

    public void Recalculate()
    {
        TotalBonuses = LineItems.Where(l => l.Type == Enums.PayslipLineItemType.Bonus).Sum(l => l.Amount);
        TotalDeductions = LineItems.Where(l => l.Type == Enums.PayslipLineItemType.Deduction).Sum(l => l.Amount);
        NetPay = GrossPay + TotalBonuses - TotalDeductions;
    }
}
