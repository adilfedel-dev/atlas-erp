using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AtlasERP.Infrastructure.PerCompany;

/// <summary>
/// Lets `dotnet ef migrations add` generate migrations for the per-company schema
/// without a real company selected. All company databases share one schema, so any
/// valid connection string works here — point ConnectionStrings:CompanyDbTemplate at a
/// scratch/dev database in appsettings.json.
/// </summary>
public class CompanyDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CompanyDbContext>
{
    public CompanyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("../AtlasERP.Presentation.WPF/appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("CompanyDbTemplate")
            ?? "Server=(localdb)\\mssqllocaldb;Database=AtlasERP_CompanyTemplate;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CompanyDbContext(optionsBuilder.Options);
    }
}
