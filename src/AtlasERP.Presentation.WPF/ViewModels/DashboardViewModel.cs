using AtlasERP.Core.Application.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AtlasERP.Presentation.WPF.ViewModels;

/// <summary>
/// Placeholder landing page. Will grow into real widgets (headcount, pending approvals,
/// upcoming payroll runs, etc.) as those modules land.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    public DashboardViewModel(ICompanyContextService companyContextService)
    {
        var companyName = companyContextService.CurrentCompany?.Name ?? "your company";
        WelcomeMessage = $"Welcome to AtlasERP — {companyName}";
    }
}
