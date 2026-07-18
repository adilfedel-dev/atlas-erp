using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.Company;

/// <summary>
/// Every method opens its own CompanyDbContext against whatever company is currently
/// selected — never held across calls, since the user can switch companies between them.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly ICompanyDbContextFactory _companyDbContextFactory;

    public EmployeeService(ICompanyDbContextFactory companyDbContextFactory)
    {
        _companyDbContextFactory = companyDbContextFactory;
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        return await db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        db.Employees.Add(employee);
        await db.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        db.Employees.Update(employee);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = _companyDbContextFactory.CreateDbContextForCurrentCompany();
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (employee is not null)
        {
            db.Employees.Remove(employee);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
