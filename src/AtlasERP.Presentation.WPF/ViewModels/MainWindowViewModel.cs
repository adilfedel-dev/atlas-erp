using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Presentation.WPF.Navigation;
using AtlasERP.Presentation.WPF.Themes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
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

    [ObservableProperty]
    private bool _isDashboardActive = true;

    [ObservableProperty]
    private bool _isEmployeesActive;

    [ObservableProperty]
    private bool _isContractsActive;

    [ObservableProperty]
    private bool _isPayrollActive;

    [ObservableProperty]
    private bool _isCustomersActive;

    [ObservableProperty]
    private bool _isInvoicesActive;

    [ObservableProperty]
    private bool _isSettingsActive;

    [ObservableProperty]
    private bool _isTravelExpensesActive;

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
        SetActiveNav(dashboard: true);
    }

    [RelayCommand]
    private void NavigateToEmployees()
    {
        CurrentPage = _serviceProvider.GetRequiredService<EmployeeListViewModel>();
        CurrentPageTitle = "Employees";
        SetActiveNav(employees: true);
    }

    [RelayCommand]
    private void NavigateToContracts()
    {
        CurrentPage = _serviceProvider.GetRequiredService<ContractListViewModel>();
        CurrentPageTitle = "Contracts";
        SetActiveNav(contracts: true);
    }

    [RelayCommand]
    private void NavigateToPayroll()
    {
        CurrentPage = _serviceProvider.GetRequiredService<PayrollRunListViewModel>();
        CurrentPageTitle = "Payroll";
        SetActiveNav(payroll: true);
    }

    [RelayCommand]
    private void NavigateToCustomers()
    {
        CurrentPage = _serviceProvider.GetRequiredService<CustomerListViewModel>();
        CurrentPageTitle = "Customers";
        SetActiveNav(customers: true);
    }

    [RelayCommand]
    private void NavigateToInvoices()
    {
        CurrentPage = _serviceProvider.GetRequiredService<InvoiceListViewModel>();
        CurrentPageTitle = "Invoices";
        SetActiveNav(invoices: true);
    }

    [RelayCommand]
    private void NavigateToTravelExpenses()
    {
        CurrentPage = _serviceProvider.GetRequiredService<TravelExpenseListViewModel>();
        CurrentPageTitle = "Travel Expenses";
        SetActiveNav(travelExpenses: true);
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = _serviceProvider.GetRequiredService<SettingsViewModel>();
        CurrentPageTitle = "Settings";
        SetActiveNav(settings: true);
    }

    private void SetActiveNav(bool dashboard = false, bool employees = false, bool contracts = false, bool payroll = false, bool customers = false, bool invoices = false, bool settings = false, bool travelExpenses = false)
    {
        IsDashboardActive = dashboard;
        IsEmployeesActive = employees;
        IsContractsActive = contracts;
        IsPayrollActive = payroll;
        IsCustomersActive = customers;
        IsInvoicesActive = invoices;
        IsSettingsActive = settings;
        IsTravelExpensesActive = travelExpenses;
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

    /// <summary>
    /// Unsubscribes from the (singleton) company context service's event. Needed now that
    /// this ViewModel is created fresh per login rather than once for the app's lifetime —
    /// without this, every switched-company cycle would leave a stale subscriber behind.
    /// </summary>
    public void Dispose()
    {
        _companyContextService.CompanyChanged -= OnCompanyChanged;
    }
}
