using System.Collections.ObjectModel;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Payroll.Enums;
using AtlasERP.Presentation.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public record PayrollRunSummary(Guid Id, string PeriodLabel, PayrollRunStatus Status, int EmployeeCount, decimal TotalNet);

public partial class PayrollRunListViewModel : ObservableObject
{
    private readonly IPayrollService _payrollService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<PayrollRunSummary> _runs = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ViewRunCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteRunCommand))]
    private PayrollRunSummary? _selectedRun;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public PayrollRunListViewModel(IPayrollService payrollService, IServiceProvider serviceProvider)
    {
        _payrollService = payrollService;
        _serviceProvider = serviceProvider;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var runs = await _payrollService.GetAllRunsAsync();
            Runs = new ObservableCollection<PayrollRunSummary>(runs.Select(r => new PayrollRunSummary(
                r.Id,
                new DateTime(r.PeriodYear, r.PeriodMonth, 1).ToString("MMMM yyyy"),
                r.Status,
                r.Payslips.Count,
                r.Payslips.Sum(p => p.NetPay))));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load payroll runs: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GenerateRun()
    {
        var view = _serviceProvider.GetRequiredService<GenerateRunView>();
        view.Owner = Application.Current.MainWindow;

        view.ViewModel.Confirmed += async (_, _) =>
        {
            try
            {
                await _payrollService.GenerateRunAsync(view.ViewModel.Year, view.ViewModel.Month);
                view.DialogResult = true;
            }
            catch (Exception ex)
            {
                view.ViewModel.ErrorMessage = ex.Message;
            }
        };
        view.ViewModel.Cancelled += (_, _) => view.DialogResult = false;

        var result = view.ShowDialog();
        if (result == true)
        {
            _ = LoadAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task ViewRunAsync()
    {
        if (SelectedRun is null)
        {
            return;
        }

        var view = _serviceProvider.GetRequiredService<PayrollRunDetailView>();
        await view.ViewModel.InitializeAsync(SelectedRun.Id);
        view.Owner = Application.Current.MainWindow;
        view.ShowDialog();
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteRunAsync()
    {
        if (SelectedRun is null)
        {
            return;
        }

        var confirmed = MessageBox.Show(
            $"Delete the payroll run for {SelectedRun.PeriodLabel}?",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _payrollService.DeleteRunAsync(SelectedRun.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not delete run: {ex.Message}";
        }
    }

    private bool HasSelection() => SelectedRun is not null;

    private bool CanDelete() => SelectedRun is { Status: PayrollRunStatus.Draft };
}
