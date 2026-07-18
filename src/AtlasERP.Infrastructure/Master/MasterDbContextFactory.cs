using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtlasERP.Infrastructure.Master;

/// <summary>
/// Lets `dotnet ef migrations add` work against MasterDbContext without spinning up the
/// full app/DI host. Only needs a valid SQLite connection string to generate migration
/// files — the file doesn't need to exist or be reachable.
/// </summary>
public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    public MasterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
        optionsBuilder.UseSqlite("Data Source=atlaserp_master_design.db");

        return new MasterDbContext(optionsBuilder.Options);
    }
}
