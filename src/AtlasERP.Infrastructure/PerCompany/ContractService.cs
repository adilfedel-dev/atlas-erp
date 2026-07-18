using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.PerCompany;

public class ContractService : IContractService
{
    private readonly ICompanyDbContextFactory _companyDbContextFactory;

    public ContractService(ICompanyDbContextFactory companyDbContextFactory)
    {
        _companyDbContextFactory = companyDbContextFactory;
    }

    public async Task<IReadOnlyList<EmployeeContract>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.EmployeeContracts
            .AsNoTracking()
            .Include(c => c.Employee)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeContract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.EmployeeContracts
            .Include(c => c.Employee)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<EmployeeContract> CreateAsync(EmployeeContract contract, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        db.EmployeeContracts.Add(contract);
        await db.SaveChangesAsync(cancellationToken);
        return contract;
    }

    public async Task UpdateAsync(EmployeeContract contract, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        db.EmployeeContracts.Update(contract);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var contract = await db.EmployeeContracts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (contract is not null)
        {
            db.EmployeeContracts.Remove(contract);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
