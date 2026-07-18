using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Expenses;
using AtlasERP.Core.Domain.Expenses.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.PerCompany;

public class TravelExpenseService : ITravelExpenseService
{
    private readonly ICompanyDbContextFactory _companyDbContextFactory;

    public TravelExpenseService(ICompanyDbContextFactory companyDbContextFactory)
    {
        _companyDbContextFactory = companyDbContextFactory;
    }

    public async Task<IReadOnlyList<TravelExpenseReport>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.TravelExpenseReports
            .AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.LineItems)
            .OrderByDescending(r => r.DepartureDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TravelExpenseReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.TravelExpenseReports
            .Include(r => r.Employee)
            .Include(r => r.LineItems)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<TravelExpenseReport> CreateAsync(Guid employeeId, string destination, string purpose, DateTime departureDate, DateTime returnDate, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();

        var report = new TravelExpenseReport
        {
            EmployeeId = employeeId,
            Destination = destination,
            Purpose = purpose,
            DepartureDate = departureDate,
            ReturnDate = returnDate
        };

        db.TravelExpenseReports.Add(report);
        await db.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task AddLineItemAsync(Guid reportId, DateTime date, ExpenseCategory category, string description, decimal amount, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var report = await db.TravelExpenseReports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new InvalidOperationException("Travel expense report not found.");

        db.TravelExpenseLineItems.Add(new TravelExpenseLineItem
        {
            ReportId = report.Id,
            Date = date,
            Category = category,
            Description = description,
            Amount = amount
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLineItemAsync(Guid lineItemId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var lineItem = await db.TravelExpenseLineItems.FirstOrDefaultAsync(l => l.Id == lineItemId, cancellationToken);
        if (lineItem is not null)
        {
            db.TravelExpenseLineItems.Remove(lineItem);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task SubmitAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var report = await db.TravelExpenseReports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new InvalidOperationException("Travel expense report not found.");

        report.Status = TravelExpenseStatus.Submitted;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsOwnerAsync(Guid reportId, string approverName, string signaturePath, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var report = await db.TravelExpenseReports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new InvalidOperationException("Travel expense report not found.");

        report.OwnerApproverName = approverName;
        report.OwnerSignaturePath = signaturePath;
        report.OwnerApprovedAtUtc = DateTime.UtcNow;

        if (report.AccountantApprovedAtUtc is not null)
        {
            report.Status = TravelExpenseStatus.Approved;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsAccountantAsync(Guid reportId, string approverName, string signaturePath, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var report = await db.TravelExpenseReports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new InvalidOperationException("Travel expense report not found.");

        report.AccountantApproverName = approverName;
        report.AccountantSignaturePath = signaturePath;
        report.AccountantApprovedAtUtc = DateTime.UtcNow;

        if (report.OwnerApprovedAtUtc is not null)
        {
            report.Status = TravelExpenseStatus.Approved;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var report = await db.TravelExpenseReports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new InvalidOperationException("Travel expense report not found.");

        report.Status = TravelExpenseStatus.Rejected;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var report = await db.TravelExpenseReports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
        if (report is not null)
        {
            db.TravelExpenseReports.Remove(report);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
