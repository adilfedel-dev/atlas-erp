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
            .ConfigureServices((context, services) => ConfigureServices(context.Configuration, services))
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
                $"Could not connect to or prepare the Master database.\n\n{ex.Message}\n\nCheck the connection string in appsettings.json and that SQL Server / LocalDB is running.",
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

    private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddDbContextFactory<MasterDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MasterDb")));

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

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginView>();
        services.AddTransient<CompanySelectorViewModel>();
        services.AddTransient<CompanySelectorView>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<EmployeeListViewModel>();
        services.AddTransient<EmployeeEditViewModel>();
        services.AddTransient<EmployeeEditView>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
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
