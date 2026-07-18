using System.IO;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Application.Services;
using AtlasERP.Infrastructure.PerCompany;
using AtlasERP.Infrastructure.Master;
using AtlasERP.Presentation.WPF.Navigation;
using AtlasERP.Presentation.WPF.Themes;
using AtlasERP.Presentation.WPF.ViewModels;
using AtlasERP.Presentation.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AtlasERP.Presentation.WPF;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .WriteTo.File(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "atlaserp-.log"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder(e.Args)
            .UseSerilog()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices((_, services) => ConfigureServices(services))
            .Build();

        await _host.StartAsync();

        try
        {
            await RunStartupMigrationsAndSeedAsync(_host.Services);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to prepare the Master database on startup.");
            MessageBox.Show(
                $"Could not prepare the Master database.\n\n{ex.Message}",
                "AtlasERP — Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        ThemeManager.ApplyTheme(ThemeManager.CurrentTheme);

        var navigation = _host.Services.GetRequiredService<IAppNavigation>();
        navigation.ShowLogin();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextFactory<MasterDbContext>(options =>
        {
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDirectory);
            var masterDbPath = Path.Combine(dataDirectory, "AtlasERP_Master.db");
            // DefaultTimeout gives concurrent short-lived connections room to wait for a
            // lock to clear instead of failing immediately — see CompanyDbContextFactory
            // for the full reasoning.
            var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
            {
                DataSource = masterDbPath,
                DefaultTimeout = 15
            }.ConnectionString;
            options.UseSqlite(connectionString);
        });

        services.AddSingleton<ICompanyContextService, CompanyContextService>();
        services.AddSingleton<ICompanyDbContextFactory, CompanyDbContextFactory>();
        services.AddSingleton<IAppNavigation, AppNavigation>();

        // Registered as singletons, not scoped: this WPF app never opens a per-operation
        // IServiceScope around view resolution, so a "scoped" registration here would just
        // be a root-resolved instance anyway. Each of these is genuinely stateless — every
        // method opens its own short-lived DbContext via IDbContextFactory / ICompanyDbContextFactory.
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ICompanyDirectoryService, CompanyDirectoryService>();
        services.AddSingleton<CompanyMigrationRunner>();
        services.AddSingleton<MasterDbSeeder>();
        services.AddSingleton<IEmployeeService, EmployeeService>();
        services.AddSingleton<IContractService, ContractService>();
        services.AddSingleton<IPayrollService, PayrollService>();
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IInvoiceService, InvoiceService>();
        services.AddSingleton<ICompanySettingsService, CompanySettingsService>();
        services.AddSingleton<ITravelExpenseService, TravelExpenseService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginView>();
        services.AddTransient<CompanySelectorViewModel>();
        services.AddTransient<CompanySelectorView>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<EmployeeListViewModel>();
        services.AddTransient<EmployeeEditViewModel>();
        services.AddTransient<EmployeeEditView>();
        services.AddTransient<ContractListViewModel>();
        services.AddTransient<ContractEditViewModel>();
        services.AddTransient<ContractEditView>();
        services.AddTransient<PayrollRunListViewModel>();
        services.AddTransient<PayrollRunDetailViewModel>();
        services.AddTransient<PayrollRunDetailView>();
        services.AddTransient<GenerateRunViewModel>();
        services.AddTransient<GenerateRunView>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CustomerEditViewModel>();
        services.AddTransient<CustomerEditView>();
        services.AddTransient<InvoiceListViewModel>();
        services.AddTransient<InvoiceCreateViewModel>();
        services.AddTransient<InvoiceCreateView>();
        services.AddTransient<InvoiceDetailViewModel>();
        services.AddTransient<InvoiceDetailView>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<TravelExpenseListViewModel>();
        services.AddTransient<TravelExpenseCreateViewModel>();
        services.AddTransient<TravelExpenseCreateView>();
        services.AddTransient<TravelExpenseDetailViewModel>();
        services.AddTransient<TravelExpenseDetailView>();
        services.AddTransient<SignatureCaptureViewModel>();
        services.AddTransient<SignatureCaptureView>();
        // Transient, not singleton: once a Window is Close()'d, WPF will not let it be
        // reopened. MainWindow gets closed every time the user logs out (or switches
        // companies, which goes through logout), so reusing a singleton instance on the
        // next login threw an unhandled exception on Show() — this was the "app lags then
        // closes when opening a different company" crash.
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }

    private static async Task RunStartupMigrationsAndSeedAsync(IServiceProvider services)
    {
        var dbContextFactory = services.GetRequiredService<IDbContextFactory<MasterDbContext>>();
        await using var masterDb = await dbContextFactory.CreateDbContextAsync();

        await masterDb.Database.MigrateAsync();

        var seeder = services.GetRequiredService<MasterDbSeeder>();
        await seeder.SeedAsync(masterDb);

        // Every registered company also needs its schema created/updated — otherwise the
        // first company a user opens has no tables yet. Loops over all companies from the
        // Master DB, so a schema change never lands in only whichever one you tested against.
        var migrationRunner = services.GetRequiredService<CompanyMigrationRunner>();
        await migrationRunner.MigrateAllCompaniesAsync();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
