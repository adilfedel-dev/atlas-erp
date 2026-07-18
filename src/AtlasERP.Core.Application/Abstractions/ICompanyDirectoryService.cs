using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Core.Application.Abstractions;

public interface ICompanyDirectoryService
{
    /// <summary>Companies the given user is allowed to open, per Master DB UserCompanyAccess entries.</summary>
    Task<IReadOnlyList<Company>> GetAccessibleCompaniesAsync(Guid userId, CancellationToken cancellationToken = default);
}
