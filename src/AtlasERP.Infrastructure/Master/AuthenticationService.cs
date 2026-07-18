using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Master;
using Microsoft.EntityFrameworkCore;

namespace AtlasERP.Infrastructure.Master;

public class AuthenticationService : IAuthenticationService
{
    private readonly IDbContextFactory<MasterDbContext> _masterDbContextFactory;

    public AuthenticationService(IDbContextFactory<MasterDbContext> masterDbContextFactory)
    {
        _masterDbContextFactory = masterDbContextFactory;
    }

    public async Task<ApplicationUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        await using var db = await _masterDbContextFactory.CreateDbContextAsync(cancellationToken);

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        user.LastLoginUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return user;
    }
}
