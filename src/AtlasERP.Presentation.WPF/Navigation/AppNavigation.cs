using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Master;
using AtlasERP.Presentation.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AtlasERP.Presentation.WPF.Navigation;

public class AppNavigation : IAppNavigation
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICompanyContextService _companyContextService;
    private readonly ILogger<AppNavigation> _logger;
    private Window? _currentWindow;

    public AppNavigation(IServiceProvider serviceProvider, ICompanyContextService companyContextService, ILogger<AppNavigation> logger)
    {
        _serviceProvider = serviceProvider;
        _companyContextService = companyContextService;
        _logger = logger;
    }

    public void ShowLogin()
    {
        var view = _serviceProvider.GetRequiredService<LoginView>();
        view.ViewModel.LoginSucceeded += OnLoginSucceeded;
        SwapWindow(view);
    }

    private async void OnLoginSucceeded(object? sender, ApplicationUser user)
    {
        try
        {
            var directory = _serviceProvider.GetRequiredService<ICompanyDirectoryService>();
            var companies = await directory.GetAccessibleCompaniesAsync(user.Id);

            if (companies.Count == 0)
            {
                MessageBox.Show(
                    "This account has no company access configured. Contact an administrator.",
                    "AtlasERP",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (companies.Count == 1)
            {
                _companyContextService.SetCurrentCompany(companies[0]);
                ShowMainWindow();
                return;
            }

            ShowCompanySelector(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve accessible companies for user {UserId}.", user.Id);
            MessageBox.Show($"Could not load your companies.\n\n{ex.Message}", "AtlasERP", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowCompanySelector(IReadOnlyList<Company> companies)
    {
        var view = _serviceProvider.GetRequiredService<CompanySelectorView>();
        view.ViewModel.Initialize(companies);
        view.ViewModel.CompanySelected += (_, company) =>
        {
            _companyContextService.SetCurrentCompany(company);
            ShowMainWindow();
        };
        SwapWindow(view);
    }

    public void ShowMainWindow()
    {
        var window = _serviceProvider.GetRequiredService<MainWindow>();
        SwapWindow(window);
    }

    private void SwapWindow(Window next)
    {
        var previous = _currentWindow;
        next.Show();
        Application.Current.MainWindow = next;
        _currentWindow = next;
        previous?.Close();
    }
}
