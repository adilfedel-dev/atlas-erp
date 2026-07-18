using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.PerCompany;

public class CustomerService : ICustomerService
{
    private readonly ICompanyDbContextFactory _companyDbContextFactory;

    public CustomerService(ICompanyDbContextFactory companyDbContextFactory)
    {
        _companyDbContextFactory = companyDbContextFactory;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer> CreateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        db.Customers.Update(customer);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer is not null)
        {
            db.Customers.Remove(customer);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
