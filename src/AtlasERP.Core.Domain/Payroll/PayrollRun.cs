using AtlasERP.Core.Domain.Common;
using AtlasERP.Core.Domain.Payroll.Enums;

namespace AtlasERP.Core.Domain.Payroll;

public class PayrollRun : EntityBase
{
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public PayrollRunStatus Status { get; set; } = PayrollRunStatus.Draft;
    public DateTime? FinalizedAtUtc { get; set; }

    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
