using AtlasERP.Core.Domain.HumanResources;

namespace AtlasERP.Core.Application.Abstractions;

public interface IContractService
{
    /// <summary>Contracts with their Employee navigation populated, for display.</summary>
    Task<IReadOnlyList<EmployeeContract>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<EmployeeContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeContract> CreateAsync(EmployeeContract contract, CancellationToken cancellationToken = default);

    Task UpdateAsync(EmployeeContract contract, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
