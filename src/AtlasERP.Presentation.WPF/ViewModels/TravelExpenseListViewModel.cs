using System.Collections.ObjectModel;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Expenses;
using AtlasERP.Presentation.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class TravelExpenseListViewModel : ObservableObject
{
    private readonly ITravelExpenseService _travelExpenseService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<TravelExpenseReport> _reports = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ViewReportCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteReportCommand))]
    private TravelExpenseReport? _selectedReport;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public TravelExpenseListViewModel(ITravelExpenseService travelExpenseService, IServiceProvider serviceProvider)
    {
        _travelExpenseService = travelExpenseService;
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
            var reports = await _travelExpenseService.GetAllAsync();
            Reports = new ObservableCollection<TravelExpenseReport>(reports);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load travel expense reports: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddReportAsync()
    {
        var view = _serviceProvider.GetRequiredService<TravelExpenseCreateView>();
        await view.ViewModel.InitializeAsync();
        view.Owner = Application.Current.MainWindow;

        Guid? createdReportId = null;
        view.ViewModel.Confirmed += async (_, _) =>
        {
            try
            {
                var report = await _travelExpenseService.CreateAsync(
                    view.ViewModel.SelectedEmployee!.Id,
                    view.ViewModel.Destination,
                    view.ViewModel.Purpose,
                    view.ViewModel.DepartureDate,
                    view.ViewModel.ReturnDate);
                createdReportId = report.Id;
                view.DialogResult = true;
            }
            catch (Exception ex)
            {
                view.ViewModel.ErrorMessage = ex.Message;
            }
        };
        view.ViewModel.Cancelled += (_, _) => view.DialogResult = false;

        var result = view.ShowDialog();
        await LoadAsync();

        if (result == true && createdReportId is not null)
        {
            await OpenDetailAsync(createdReportId.Value);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task ViewReportAsync()
    {
        if (SelectedReport is null)
        {
            return;
        }

        await OpenDetailAsync(SelectedReport.Id);
    }

    private async Task OpenDetailAsync(Guid reportId)
    {
        var view = _serviceProvider.GetRequiredService<TravelExpenseDetailView>();
        await view.ViewModel.InitializeAsync(reportId);
        view.Owner = Application.Current.MainWindow;
        view.ShowDialog();
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteReportAsync()
    {
        if (SelectedReport is null)
        {
            return;
        }

        var confirmed = MessageBox.Show(
            $"Delete the travel expense report for {SelectedReport.Employee?.FirstName} {SelectedReport.Employee?.LastName}?",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _travelExpenseService.DeleteAsync(SelectedReport.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not delete report: {ex.Message}";
        }
    }

    private bool HasSelection() => SelectedReport is not null;
}
