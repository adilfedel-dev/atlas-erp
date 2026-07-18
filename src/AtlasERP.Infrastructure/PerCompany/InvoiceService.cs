using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Sales;
using AtlasERP.Core.Domain.Sales.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.PerCompany;

public class InvoiceService : IInvoiceService
{
    private readonly ICompanyDbContextFactory _companyDbContextFactory;

    public InvoiceService(ICompanyDbContextFactory companyDbContextFactory)
    {
        _companyDbContextFactory = companyDbContextFactory;
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .Include(i => i.Receipts)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.LineItems)
            .Include(i => i.Receipts)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice> CreateAsync(Guid customerId, DateTime issueDate, DateTime dueDate, decimal taxRatePercent, string? notes, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();

        var invoiceCount = await db.Invoices.CountAsync(cancellationToken);
        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-{invoiceCount + 1:D4}",
            CustomerId = customerId,
            IssueDate = issueDate,
            DueDate = dueDate,
            TaxRatePercent = taxRatePercent,
            Notes = notes
        };
        invoice.Recalculate();

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task AddLineItemAsync(Guid invoiceId, string description, decimal quantity, decimal unitPrice, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var invoice = await db.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        invoice.LineItems.Add(new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice
        });
        invoice.Recalculate();

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLineItemAsync(Guid lineItemId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var lineItem = await db.InvoiceLineItems
            .Include(l => l.Invoice)
            .ThenInclude(i => i!.LineItems)
            .FirstOrDefaultAsync(l => l.Id == lineItemId, cancellationToken);

        if (lineItem?.Invoice is null)
        {
            return;
        }

        var invoice = lineItem.Invoice;
        invoice.LineItems.Remove(lineItem);
        db.InvoiceLineItems.Remove(lineItem);
        invoice.Recalculate();

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid invoiceId, InvoiceStatus status, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        invoice.Status = status;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordReceiptAsync(Guid invoiceId, decimal amount, DateTime receivedDate, PaymentMethod paymentMethod, string? notes, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var invoice = await db.Invoices
            .Include(i => i.Receipts)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        invoice.Receipts.Add(new Receipt
        {
            InvoiceId = invoice.Id,
            Amount = amount,
            ReceivedDate = receivedDate,
            PaymentMethod = paymentMethod,
            Notes = notes
        });

        var totalPaid = invoice.Receipts.Sum(r => r.Amount);
        if (totalPaid >= invoice.Total && invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Sent or InvoiceStatus.Overdue)
        {
            invoice.Status = InvoiceStatus.Paid;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        if (invoice is not null)
        {
            db.Invoices.Remove(invoice);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
