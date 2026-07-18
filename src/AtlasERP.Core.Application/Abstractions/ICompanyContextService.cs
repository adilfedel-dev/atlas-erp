using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Core.Application.Abstractions;

/// <summary>
/// Tracks which company is "active" for the current session. Nothing here touches a
/// database — it's pure in-memory state that the rest of the app reacts to when the
/// user switches companies (or logs in and picks one for the first time).
/// </summary>
public interface ICompanyContextService
{
    Company? CurrentCompany { get; }

    event EventHandler<Company?>? CompanyChanged;

    void SetCurrentCompany(Company company);

    void ClearCurrentCompany();
}
