using AtlasERP.Core.Domain.Expenses;
using AtlasERP.Core.Domain.Expenses.Enums;

namespace AtlasERP.Core.Application.Abstractions;

public interface ITravelExpenseService
{
    /// <summary>Reports with Employee and LineItems populated.</summary>
    Task<IReadOnlyList<TravelExpenseReport>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TravelExpenseReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TravelExpenseReport> CreateAsync(Guid employeeId, string destination, string purpose, DateTime departureDate, DateTime returnDate, CancellationToken cancellationToken = default);

    Task AddLineItemAsync(Guid reportId, DateTime date, ExpenseCategory category, string description, decimal amount, CancellationToken cancellationToken = default);

    Task RemoveLineItemAsync(Guid lineItemId, CancellationToken cancellationToken = default);

    Task SubmitAsync(Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>Records the owner's signature; marks the report Approved once both signatures are present.</summary>
    Task ApproveAsOwnerAsync(Guid reportId, string approverName, string signaturePath, CancellationToken cancellationToken = default);

    /// <summary>Records the accountant's signature; marks the report Approved once both signatures are present.</summary>
    Task ApproveAsAccountantAsync(Guid reportId, string approverName, string signaturePath, CancellationToken cancellationToken = default);

    Task RejectAsync(Guid reportId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid reportId, CancellationToken cancellationToken = default);
}
