using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources.Enums;
using AtlasERP.Core.Domain.Payroll;
using AtlasERP.Core.Domain.Payroll.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.PerCompany;

public class PayrollService : IPayrollService
{
    private readonly ICompanyDbContextFactory _companyDbContextFactory;

    public PayrollService(ICompanyDbContextFactory companyDbContextFactory)
    {
        _companyDbContextFactory = companyDbContextFactory;
    }

    public async Task<IReadOnlyList<PayrollRun>> GetAllRunsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.PayrollRuns
            .AsNoTracking()
            .Include(r => r.Payslips)
            .OrderByDescending(r => r.PeriodYear)
            .ThenByDescending(r => r.PeriodMonth)
            .ToListAsync(cancellationToken);
    }

    public async Task<PayrollRun?> GetRunByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.PayrollRuns
            .Include(r => r.Payslips).ThenInclude(p => p.Employee)
            .Include(r => r.Payslips).ThenInclude(p => p.LineItems)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<PayrollRun> GenerateRunAsync(int periodYear, int periodMonth, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();

        var alreadyExists = await db.PayrollRuns
            .AnyAsync(r => r.PeriodYear == periodYear && r.PeriodMonth == periodMonth, cancellationToken);
        if (alreadyExists)
        {
            throw new InvalidOperationException($"A payroll run for {periodYear}-{periodMonth:D2} already exists.");
        }

        var activeEmployees = await db.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
            .ToListAsync(cancellationToken);

        var run = new PayrollRun
        {
            PeriodYear = periodYear,
            PeriodMonth = periodMonth
        };

        foreach (var employee in activeEmployees)
        {
            var payslip = new Payslip
            {
                PayrollRun = run,
                EmployeeId = employee.Id,
                GrossPay = employee.BaseSalary
            };
            payslip.Recalculate();
            run.Payslips.Add(payslip);
        }

        db.PayrollRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        return run;
    }

    public async Task FinalizeRunAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var run = await db.PayrollRuns.FirstOrDefaultAsync(r => r.Id == runId, cancellationToken)
            ?? throw new InvalidOperationException("Payroll run not found.");

        run.Status = PayrollRunStatus.Finalized;
        run.FinalizedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRunAsync(Guid runId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var run = await db.PayrollRuns.FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);
        if (run is null)
        {
            return;
        }

        if (run.Status == PayrollRunStatus.Finalized)
        {
            throw new InvalidOperationException("Finalized payroll runs cannot be deleted.");
        }

        db.PayrollRuns.Remove(run);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLineItemAsync(Guid payslipId, PayslipLineItemType type, string description, decimal amount, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var payslip = await db.Payslips
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == payslipId, cancellationToken)
            ?? throw new InvalidOperationException("Payslip not found.");

        payslip.LineItems.Add(new PayslipLineItem
        {
            PayslipId = payslip.Id,
            Type = type,
            Description = description,
            Amount = amount
        });
        payslip.Recalculate();

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLineItemAsync(Guid lineItemId, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var lineItem = await db.PayslipLineItems
            .Include(l => l.Payslip)
            .ThenInclude(p => p!.LineItems)
            .FirstOrDefaultAsync(l => l.Id == lineItemId, cancellationToken);

        if (lineItem?.Payslip is null)
        {
            return;
        }

        var payslip = lineItem.Payslip;
        payslip.LineItems.Remove(lineItem);
        db.PayslipLineItems.Remove(lineItem);
        payslip.Recalculate();

        await db.SaveChangesAsync(cancellationToken);
    }
}
