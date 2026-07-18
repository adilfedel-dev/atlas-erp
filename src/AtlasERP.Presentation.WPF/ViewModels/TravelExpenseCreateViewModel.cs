using System.Collections.ObjectModel;
using AtlasERP.Core.Application.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class TravelExpenseCreateViewModel : ObservableObject
{
    private readonly IEmployeeService _employeeService;

    [ObservableProperty] private ObservableCollection<EmployeeOption> _employeeOptions = new();
    [ObservableProperty] private EmployeeOption? _selectedEmployee;
    [ObservableProperty] private string _destination = string.Empty;
    [ObservableProperty] private string _purpose = string.Empty;
    [ObservableProperty] private DateTime _departureDate = DateTime.Today;
    [ObservableProperty] private DateTime _returnDate = DateTime.Today.AddDays(1);
    [ObservableProperty] private string? _errorMessage;

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    public TravelExpenseCreateViewModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public async Task InitializeAsync()
    {
        var employees = await _employeeService.GetAllAsync();
        EmployeeOptions = new ObservableCollection<EmployeeOption>(employees.Select(e => new EmployeeOption(e.Id, $"{e.FirstName} {e.LastName} ({e.EmployeeCode})")));
    }

    [RelayCommand]
    private void Confirm()
    {
        if (SelectedEmployee is null)
        {
            ErrorMessage = "Select an employee.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Destination) || string.IsNullOrWhiteSpace(Purpose))
        {
            ErrorMessage = "Enter a destination and purpose.";
            return;
        }

        if (ReturnDate < DepartureDate)
        {
            ErrorMessage = "Return date must be on or after the departure date.";
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
