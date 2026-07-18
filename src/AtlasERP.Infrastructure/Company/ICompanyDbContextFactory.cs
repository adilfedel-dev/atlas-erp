using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Infrastructure.Company;

/// <summary>
/// Creates a <see cref="CompanyDbContext"/> targeting a specific company's database.
/// Exists because a plain injected DbContext is fixed to one connection string for the
/// app's lifetime, but here the target database changes whenever the user switches
/// companies mid-session.
/// </summary>
public interface ICompanyDbContextFactory
{
    CompanyDbContext CreateDbContext(Core.Domain.Master.Company company);

    /// <summary>
    /// Convenience overload for the common case: build a context for whatever
    /// ICompanyContextService currently reports as active. Throws if no company is selected.
    /// </summary>
    CompanyDbContext CreateDbContextForCurrentCompany();
}
