using System.Collections.ObjectModel;
using AtlasERP.Core.Application.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public record CustomerOption(Guid Id, string Name);

public partial class InvoiceCreateViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;

    [ObservableProperty] private ObservableCollection<CustomerOption> _customerOptions = new();
    [ObservableProperty] private CustomerOption? _selectedCustomer;
    [ObservableProperty] private DateTime _issueDate = DateTime.Today;
    [ObservableProperty] private DateTime _dueDate = DateTime.Today.AddDays(30);
    [ObservableProperty] private decimal _taxRatePercent;
    [ObservableProperty] private string? _errorMessage;

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    public InvoiceCreateViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task InitializeAsync()
    {
        var customers = await _customerService.GetAllAsync();
        CustomerOptions = new ObservableCollection<CustomerOption>(customers.Select(c => new CustomerOption(c.Id, c.Name)));
    }

    [RelayCommand]
    private void Confirm()
    {
        if (SelectedCustomer is null)
        {
            ErrorMessage = "Select a customer.";
            return;
        }

        if (DueDate < IssueDate)
        {
            ErrorMessage = "Due date must be on or after the issue date.";
            return;
        }

        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
