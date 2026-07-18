using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources;
using AtlasERP.Core.Domain.HumanResources.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class EmployeeEditViewModel : ObservableObject
{
    private readonly IEmployeeService _employeeService;
    private Guid? _employeeId;

    [ObservableProperty] private string _title = "New employee";
    [ObservableProperty] private string _employeeCode = string.Empty;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private Gender _gender = Gender.Unspecified;
    [ObservableProperty] private DateTime? _dateOfBirth;
    [ObservableProperty] private string? _nationalId;
    [ObservableProperty] private string? _personalEmail;
    [ObservableProperty] private string? _workEmail;
    [ObservableProperty] private string? _phoneNumber;
    [ObservableProperty] private string? _address;
    [ObservableProperty] private DateTime _hireDate = DateTime.Today;
    [ObservableProperty] private EmploymentStatus _employmentStatus = EmploymentStatus.Active;
    [ObservableProperty] private string _jobTitle = string.Empty;
    [ObservableProperty] private decimal _baseSalary;
    [ObservableProperty] private string? _bankName;
    [ObservableProperty] private string? _bankAccountNumber;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public IReadOnlyList<Gender> GenderOptions { get; } = Enum.GetValues<Gender>();
    public IReadOnlyList<EmploymentStatus> EmploymentStatusOptions { get; } = Enum.GetValues<EmploymentStatus>();

    public event EventHandler? Saved;
    public event EventHandler? Cancelled;

    public EmployeeEditViewModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public void InitializeForCreate()
    {
        _employeeId = null;
        Title = "New employee";
    }

    public void InitializeForEdit(Employee employee)
    {
        _employeeId = employee.Id;
        Title = $"Edit {employee.FirstName} {employee.LastName}";
        EmployeeCode = employee.EmployeeCode;
        FirstName = employee.FirstName;
        LastName = employee.LastName;
        Gender = employee.Gender;
        DateOfBirth = employee.DateOfBirth;
        NationalId = employee.NationalId;
        PersonalEmail = employee.PersonalEmail;
        WorkEmail = employee.WorkEmail;
        PhoneNumber = employee.PhoneNumber;
        Address = employee.Address;
        HireDate = employee.HireDate;
        EmploymentStatus = employee.EmploymentStatus;
        JobTitle = employee.JobTitle;
        BaseSalary = employee.BaseSalary;
        BankName = employee.BankName;
        BankAccountNumber = employee.BankAccountNumber;
        Notes = employee.Notes;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EmployeeCode) || string.IsNullOrWhiteSpace(FirstName)
            || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(JobTitle))
        {
            ErrorMessage = "Employee code, first name, last name, and job title are required.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (_employeeId is null)
            {
                await _employeeService.CreateAsync(BuildEmployee());
            }
            else
            {
                var employee = await _employeeService.GetByIdAsync(_employeeId.Value)
                    ?? throw new InvalidOperationException("This employee no longer exists.");

                ApplyFieldsTo(employee);
                await _employeeService.UpdateAsync(employee);
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

    private Employee BuildEmployee()
    {
        var employee = new Employee();
        ApplyFieldsTo(employee);
        return employee;
    }

    private void ApplyFieldsTo(Employee employee)
    {
        employee.EmployeeCode = EmployeeCode;
        employee.FirstName = FirstName;
        employee.LastName = LastName;
        employee.Gender = Gender;
        employee.DateOfBirth = DateOfBirth;
        employee.NationalId = NationalId;
        employee.PersonalEmail = PersonalEmail;
        employee.WorkEmail = WorkEmail;
        employee.PhoneNumber = PhoneNumber;
        employee.Address = Address;
        employee.HireDate = HireDate;
        employee.EmploymentStatus = EmploymentStatus;
        employee.JobTitle = JobTitle;
        employee.BaseSalary = BaseSalary;
        employee.BankName = BankName;
        employee.BankAccountNumber = BankAccountNumber;
        employee.Notes = Notes;
    }
}
