using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AtlasERP.Infrastructure.Master;

/// <summary>
/// Lets `dotnet ef migrations add` work against MasterDbContext without spinning up the
/// full app/DI host. Reads the connection string from Presentation.WPF/appsettings.json
/// since that's where the real one lives; falls back to localdb for a bare `dotnet ef` run.
/// </summary>
public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("../AtlasERP.Presentation.WPF/appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("MasterDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=AtlasERP_Master;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MasterDbContext(optionsBuilder.Options);
    }
}
