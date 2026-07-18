using AtlasERP.Core.Application.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.PerCompany;

public class CompanyDbContextFactory : ICompanyDbContextFactory
{
    private readonly ICompanyContextService _companyContextService;

    public CompanyDbContextFactory(ICompanyContextService companyContextService)
    {
        _companyContextService = companyContextService;
    }

    public CompanyDbContext CreateDbContext(Core.Domain.Master.Company company)
    {
        ArgumentNullException.ThrowIfNull(company);

        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        optionsBuilder.UseSqlite(WithBusyTimeout(company.ConnectionString));

        return new CompanyDbContext(optionsBuilder.Options);
    }

    public CompanyDbContext CreateDbContextForCurrentCompany()
    {
        var current = _companyContextService.CurrentCompany
            ?? throw new InvalidOperationException(
                "No company is currently selected. Call ICompanyContextService.SetCurrentCompany first.");

        return CreateDbContext(current);
    }

    /// <summary>
    /// This app opens a fresh short-lived SQLite connection per service call, so it's normal
    /// for two to briefly overlap (e.g. a dialog's save landing while the dashboard's
    /// background chart query is still running). Without a busy timeout, SQLite fails a
    /// write immediately instead of waiting for the other connection to finish — which is
    /// what was surfacing as a confusing "expected 1 row, affected 0" EF error.
    /// </summary>
    private static string WithBusyTimeout(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            DefaultTimeout = 15
        };
        return builder.ConnectionString;
    }
}
