using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Core.Application.Services;

/// <summary>
/// Registered as a singleton in DI so every viewmodel/service sees the same "current
/// company" without passing it around explicitly.
/// </summary>
public class CompanyContextService : ICompanyContextService
{
    public Company? CurrentCompany { get; private set; }

    public event EventHandler<Company?>? CompanyChanged;

    public void SetCurrentCompany(Company company)
    {
        ArgumentNullException.ThrowIfNull(company);
        CurrentCompany = company;
        CompanyChanged?.Invoke(this, CurrentCompany);
    }

    public void ClearCurrentCompany()
    {
        CurrentCompany = null;
        CompanyChanged?.Invoke(this, null);
    }
}
