using AtlasERP.Core.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.Company;

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
        optionsBuilder.UseSqlServer(company.ConnectionString);

        return new CompanyDbContext(optionsBuilder.Options);
    }

    public CompanyDbContext CreateDbContextForCurrentCompany()
    {
        var current = _companyContextService.CurrentCompany
            ?? throw new InvalidOperationException(
                "No company is currently selected. Call ICompanyContextService.SetCurrentCompany first.");

        return CreateDbContext(current);
    }
}
