using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

/// <summary>
/// Landing page with a few real stats pulled from the current company's data. Will grow
/// with more tiles (pending approvals, upcoming payroll runs, outstanding invoices, etc.)
/// as those modules land.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IEmployeeService _employeeService;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _totalEmployeeCount;

    [ObservableProperty]
    private int _activeEmployeeCount;

    [ObservableProperty]
    private int _onLeaveEmployeeCount;

    public DashboardViewModel(ICompanyContextService companyContextService, IEmployeeService employeeService)
    {
        _employeeService = employeeService;
        var companyName = companyContextService.CurrentCompany?.Name ?? "your company";
        WelcomeMessage = $"Welcome back — here's what's happening at {companyName}";

        _ = LoadStatsAsync();
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        IsBusy = true;
        try
        {
            var employees = await _employeeService.GetAllAsync();
            TotalEmployeeCount = employees.Count;
            ActiveEmployeeCount = employees.Count(e => e.EmploymentStatus == EmploymentStatus.Active);
            OnLeaveEmployeeCount = employees.Count(e => e.EmploymentStatus == EmploymentStatus.OnLeave);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
