using AtlasERP.Core.Domain.Common;
using AtlasERP.Core.Domain.Sales.Enums;

namespace AtlasERP.Core.Domain.Sales;

public class Invoice : EntityBase
{
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public decimal TaxRatePercent { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public void Recalculate()
    {
        Subtotal = LineItems.Sum(l => l.LineTotal);
        TaxAmount = Math.Round(Subtotal * TaxRatePercent / 100m, 2);
        Total = Subtotal + TaxAmount;
    }
}
