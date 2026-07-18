using AtlasERP.Core.Domain.Sales;
using AtlasERP.Core.Domain.Sales.Enums;

namespace AtlasERP.Core.Application.Abstractions;

public interface IInvoiceService
{
    /// <summary>Invoices with Customer, LineItems, and Receipts populated.</summary>
    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a Draft invoice with an auto-generated invoice number.</summary>
    Task<Invoice> CreateAsync(Guid customerId, DateTime issueDate, DateTime dueDate, decimal taxRatePercent, string? notes, CancellationToken cancellationToken = default);

    Task AddLineItemAsync(Guid invoiceId, string description, decimal quantity, decimal unitPrice, CancellationToken cancellationToken = default);

    Task RemoveLineItemAsync(Guid lineItemId, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Guid invoiceId, InvoiceStatus status, CancellationToken cancellationToken = default);

    /// <summary>Records a payment against the invoice; marks it Paid once receipts cover the total.</summary>
    Task RecordReceiptAsync(Guid invoiceId, decimal amount, DateTime receivedDate, PaymentMethod paymentMethod, string? notes, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
