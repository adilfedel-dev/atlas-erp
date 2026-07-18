using AtlasERP.Core.Domain.Sales.Enums;

namespace AtlasERP.Core.Domain.Sales;

/// <summary>A payment recorded against an invoice. An invoice can have several (partial payments).</summary>
public class Receipt
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public decimal Amount { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.Today;
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
}
