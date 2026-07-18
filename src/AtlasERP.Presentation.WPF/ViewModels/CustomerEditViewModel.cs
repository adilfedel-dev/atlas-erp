using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Sales;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class CustomerEditViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private Guid? _customerId;

    [ObservableProperty] private string _title = "New customer";
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private string? _taxId;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public event EventHandler? Saved;
    public event EventHandler? Cancelled;

    public CustomerEditViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public void InitializeForCreate()
    {
        _customerId = null;
        Title = "New customer";
    }

    public void InitializeForEdit(Customer customer)
    {
        _customerId = customer.Id;
        Title = $"Edit {customer.Name}";
        Name = customer.Name;
        Email = customer.Email;
        Phone = customer.Phone;
        Address = customer.Address;
        TaxId = customer.TaxId;
        Notes = customer.Notes;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (_customerId is null)
            {
                await _customerService.CreateAsync(BuildCustomer());
            }
            else
            {
                var customer = await _customerService.GetByIdAsync(_customerId.Value)
                    ?? throw new InvalidOperationException("This customer no longer exists.");

                ApplyFieldsTo(customer);
                await _customerService.UpdateAsync(customer);
            }

            Saved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not save: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private Customer BuildCustomer()
    {
        var customer = new Customer();
        ApplyFieldsTo(customer);
        return customer;
    }

    private void ApplyFieldsTo(Customer customer)
    {
        customer.Name = Name;
        customer.Email = Email;
        customer.Phone = Phone;
        customer.Address = Address;
        customer.TaxId = TaxId;
        customer.Notes = Notes;
    }
}
