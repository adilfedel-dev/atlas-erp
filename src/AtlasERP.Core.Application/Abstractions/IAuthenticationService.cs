using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Core.Application.Abstractions;

public interface IAuthenticationService
{
    /// <summary>Returns the matching user, or null if the username/password pair is invalid.</summary>
    Task<ApplicationUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
