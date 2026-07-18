using AtlasERP.Core.Domain.Payroll;
using AtlasERP.Core.Domain.Payroll.Enums;

namespace AtlasERP.Core.Application.Abstractions;

public interface IPayrollService
{
    /// <summary>Runs with their Payslips populated (for headcount/total display), newest period first.</summary>
    Task<IReadOnlyList<PayrollRun>> GetAllRunsAsync(CancellationToken cancellationToken = default);

    /// <summary>A single run with Payslips, each Payslip's Employee, and LineItems all populated.</summary>
    Task<PayrollRun?> GetRunByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a Draft run for the given period and generates one Payslip per active employee (gross = current BaseSalary).</summary>
    Task<PayrollRun> GenerateRunAsync(int periodYear, int periodMonth, CancellationToken cancellationToken = default);

    Task FinalizeRunAsync(Guid runId, CancellationToken cancellationToken = default);

    /// <summary>Only Draft runs can be deleted.</summary>
    Task DeleteRunAsync(Guid runId, CancellationToken cancellationToken = default);

    Task AddLineItemAsync(Guid payslipId, PayslipLineItemType type, string description, decimal amount, CancellationToken cancellationToken = default);

    Task RemoveLineItemAsync(Guid lineItemId, CancellationToken cancellationToken = default);
}
