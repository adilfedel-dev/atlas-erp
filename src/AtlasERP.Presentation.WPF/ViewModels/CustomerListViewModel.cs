using System.Collections.ObjectModel;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Sales;
using AtlasERP.Presentation.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class CustomerListViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCustomerCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCustomerCommand))]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public CustomerListViewModel(ICustomerService customerService, IServiceProvider serviceProvider)
    {
        _customerService = customerService;
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
            var customers = await _customerService.GetAllAsync();
            Customers = new ObservableCollection<Customer>(customers);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load customers: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddCustomer()
    {
        var view = _serviceProvider.GetRequiredService<CustomerEditView>();
        view.ViewModel.InitializeForCreate();
        ShowEditDialog(view);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void EditCustomer()
    {
        if (SelectedCustomer is null)
        {
            return;
        }

        var view = _serviceProvider.GetRequiredService<CustomerEditView>();
        view.ViewModel.InitializeForEdit(SelectedCustomer);
        ShowEditDialog(view);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer is null)
        {
            return;
        }

        var confirmed = MessageBox.Show(
            $"Delete {SelectedCustomer.Name}? This cannot be undone.",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _customerService.DeleteAsync(SelectedCustomer.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not delete customer: {ex.Message}";
        }
    }

    private bool HasSelection() => SelectedCustomer is not null;

    private void ShowEditDialog(CustomerEditView view)
    {
        view.Owner = Application.Current.MainWindow;
        var saved = view.ShowDialog();
        if (saved == true)
        {
            _ = LoadAsync();
        }
    }
}
