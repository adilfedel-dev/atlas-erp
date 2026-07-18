using AtlasERP.Core.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.Master;

public class CompanySettingsService : ICompanySettingsService
{
    private readonly IDbContextFactory<MasterDbContext> _masterDbContextFactory;

    public CompanySettingsService(IDbContextFactory<MasterDbContext> masterDbContextFactory)
    {
        _masterDbContextFactory = masterDbContextFactory;
    }

    public async Task UpdateBrandingAsync(Guid companyId, string name, string legalName, string? logoPath, string? brandColorHex, CancellationToken cancellationToken = default)
    {
        await using var db = await _masterDbContextFactory.CreateDbContextAsync(cancellationToken);
        var company = await db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Company not found.");

        company.Name = name;
        company.LegalName = legalName;
        company.LogoPath = logoPath;
        company.BrandColorHex = brandColorHex;

        await db.SaveChangesAsync(cancellationToken);
    }
}
