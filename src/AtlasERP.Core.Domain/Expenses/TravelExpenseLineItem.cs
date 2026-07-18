using AtlasERP.Core.Domain.Expenses.Enums;

namespace AtlasERP.Core.Domain.Expenses;

public class TravelExpenseLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReportId { get; set; }
    public TravelExpenseReport? Report { get; set; }

    public DateTime Date { get; set; }
    public ExpenseCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
