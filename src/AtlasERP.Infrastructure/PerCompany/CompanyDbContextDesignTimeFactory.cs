using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtlasERP.Infrastructure.PerCompany;

/// <summary>
/// Lets `dotnet ef migrations add` generate migrations for the per-company schema
/// without a real company selected. All company databases share one schema, so any
/// valid SQLite connection string works here — the file doesn't need to exist.
/// </summary>
public class CompanyDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CompanyDbContext>
{
    public CompanyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        optionsBuilder.UseSqlite("Data Source=atlaserp_company_design.db");

        return new CompanyDbContext(optionsBuilder.Options);
    }
}
