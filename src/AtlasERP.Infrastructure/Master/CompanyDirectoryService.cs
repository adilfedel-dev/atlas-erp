using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Master;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.Master;

public class CompanyDirectoryService : ICompanyDirectoryService
{
    private readonly IDbContextFactory<MasterDbContext> _masterDbContextFactory;

    public CompanyDirectoryService(IDbContextFactory<MasterDbContext> masterDbContextFactory)
    {
        _masterDbContextFactory = masterDbContextFactory;
    }

    public async Task<IReadOnlyList<Company>> GetAccessibleCompaniesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var db = await _masterDbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.UserCompanyAccess
            .Where(a => a.UserId == userId && a.Company.IsActive)
            .Select(a => a.Company)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
