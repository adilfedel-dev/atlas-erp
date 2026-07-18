using AtlasERP.Infrastructure.Master;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.Company;

/// <summary>
/// Applies pending EF Core migrations to every registered company's database in one
/// pass, so a schema change never lands in just whichever company DB happened to be
/// open when you tested it. Run this after `dotnet ef migrations add` and on app
/// startup (or from an admin "Update all databases" action).
/// </summary>
public class CompanyMigrationRunner
{
    private readonly IDbContextFactory<MasterDbContext> _masterDbContextFactory;
    private readonly ICompanyDbContextFactory _companyDbContextFactory;

    public CompanyMigrationRunner(IDbContextFactory<MasterDbContext> masterDbContextFactory, ICompanyDbContextFactory companyDbContextFactory)
    {
        _masterDbContextFactory = masterDbContextFactory;
        _companyDbContextFactory = companyDbContextFactory;
    }

    public async Task MigrateAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        await using var masterDb = await _masterDbContextFactory.CreateDbContextAsync(cancellationToken);

        var companies = await masterDb.Companies
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var company in companies)
        {
            await using var companyDb = _companyDbContextFactory.CreateDbContext(company);
            await companyDb.Database.MigrateAsync(cancellationToken);
        }
    }
}
