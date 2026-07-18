using AtlasERP.Core.Domain.HumanResources;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.Company;

/// <summary>
/// Schema shared identically by all four per-company databases: employees, payroll,
/// contracts, invoices, etc. Which physical database this talks to is decided at
/// construction time by <see cref="ICompanyDbContextFactory"/>, not by DI configuration —
/// the same schema, pointed at a different connection string per company.
/// </summary>
public class CompanyDbContext : DbContext
{
    public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompanyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
