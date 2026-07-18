using System.Collections.ObjectModel;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Sales;
using AtlasERP.Presentation.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class InvoiceListViewModel : ObservableObject
{
    private readonly IInvoiceService _invoiceService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<Invoice> _invoices = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ViewInvoiceCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteInvoiceCommand))]
    private Invoice? _selectedInvoice;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public InvoiceListViewModel(IInvoiceService invoiceService, IServiceProvider serviceProvider)
    {
        _invoiceService = invoiceService;
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
            var invoices = await _invoiceService.GetAllAsync();
            Invoices = new ObservableCollection<Invoice>(invoices);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load invoices: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddInvoiceAsync()
    {
        var view = _serviceProvider.GetRequiredService<InvoiceCreateView>();
        await view.ViewModel.InitializeAsync();
        view.Owner = Application.Current.MainWindow;

        Guid? createdInvoiceId = null;
        view.ViewModel.Confirmed += async (_, _) =>
        {
            try
            {
                var invoice = await _invoiceService.CreateAsync(
                    view.ViewModel.SelectedCustomer!.Id,
                    view.ViewModel.IssueDate,
                    view.ViewModel.DueDate,
                    view.ViewModel.TaxRatePercent,
                    null);
                createdInvoiceId = invoice.Id;
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

        if (result == true && createdInvoiceId is not null)
        {
            await OpenDetailAsync(createdInvoiceId.Value);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task ViewInvoiceAsync()
    {
        if (SelectedInvoice is null)
        {
            return;
        }

        await OpenDetailAsync(SelectedInvoice.Id);
    }

    private async Task OpenDetailAsync(Guid invoiceId)
    {
        var view = _serviceProvider.GetRequiredService<InvoiceDetailView>();
        await view.ViewModel.InitializeAsync(invoiceId);
        view.Owner = Application.Current.MainWindow;
        view.ShowDialog();
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteInvoiceAsync()
    {
        if (SelectedInvoice is null)
        {
            return;
        }

        var confirmed = MessageBox.Show(
            $"Delete invoice {SelectedInvoice.InvoiceNumber}? This cannot be undone.",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _invoiceService.DeleteAsync(SelectedInvoice.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not delete invoice: {ex.Message}";
        }
    }

    private bool HasSelection() => SelectedInvoice is not null;
}
