using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Presentation.WPF.Navigation;
using AtlasERP.Presentation.WPF.Themes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ICompanyContextService _companyContextService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppNavigation _appNavigation;

    [ObservableProperty]
    private string _currentCompanyName = string.Empty;

    [ObservableProperty]
    private string _currentPageTitle = "Dashboard";

    [ObservableProperty]
    private object? _currentPage;

    public MainWindowViewModel(ICompanyContextService companyContextService, IServiceProvider serviceProvider, IAppNavigation appNavigation)
    {
        _companyContextService = companyContextService;
        _serviceProvider = serviceProvider;
        _appNavigation = appNavigation;

        _companyContextService.CompanyChanged += OnCompanyChanged;
        CurrentCompanyName = _companyContextService.CurrentCompany?.Name ?? string.Empty;

        NavigateToDashboard();
    }

    private void OnCompanyChanged(object? sender, Core.Domain.Master.Company? company)
    {
        CurrentCompanyName = company?.Name ?? string.Empty;
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentPage = _serviceProvider.GetRequiredService<DashboardViewModel>();
        CurrentPageTitle = "Dashboard";
    }

    [RelayCommand]
    private void NavigateToEmployees()
    {
        CurrentPage = _serviceProvider.GetRequiredService<EmployeeListViewModel>();
        CurrentPageTitle = "Employees";
    }

    [RelayCommand]
    private void Logout()
    {
        _companyContextService.ClearCurrentCompany();
        _appNavigation.ShowLogin();
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        ThemeManager.ToggleTheme();
    }
}
