using System.Collections.ObjectModel;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources;
using AtlasERP.Core.Domain.HumanResources.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public record EmployeeOption(Guid Id, string DisplayName);

public partial class ContractEditViewModel : ObservableObject
{
    private readonly IContractService _contractService;
    private readonly IEmployeeService _employeeService;
    private Guid? _contractId;

    [ObservableProperty] private string _title = "New contract";
    [ObservableProperty] private ObservableCollection<EmployeeOption> _employeeOptions = new();
    [ObservableProperty] private EmployeeOption? _selectedEmployee;
    [ObservableProperty] private ContractType _contractType = ContractType.Permanent;
    [ObservableProperty] private ContractStatus _status = ContractStatus.Draft;
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime? _endDate;
    [ObservableProperty] private DateTime? _signedDate;
    [ObservableProperty] private decimal _baseSalaryAtSigning;
    [ObservableProperty] private string? _terms;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    public IReadOnlyList<ContractType> ContractTypeOptions { get; } = Enum.GetValues<ContractType>();
    public IReadOnlyList<ContractStatus> StatusOptions { get; } = Enum.GetValues<ContractStatus>();

    public event EventHandler? Saved;
    public event EventHandler? Cancelled;

    public ContractEditViewModel(IContractService contractService, IEmployeeService employeeService)
    {
        _contractService = contractService;
        _employeeService = employeeService;
    }

    public async Task InitializeForCreateAsync()
    {
        _contractId = null;
        Title = "New contract";
        await LoadEmployeeOptionsAsync();
    }

    public async Task InitializeForEditAsync(EmployeeContract contract)
    {
        _contractId = contract.Id;
        Title = $"Edit contract — {contract.Employee?.FirstName} {contract.Employee?.LastName}";

        await LoadEmployeeOptionsAsync();
        SelectedEmployee = EmployeeOptions.FirstOrDefault(o => o.Id == contract.EmployeeId);

        ContractType = contract.ContractType;
        Status = contract.Status;
        StartDate = contract.StartDate;
        EndDate = contract.EndDate;
        SignedDate = contract.SignedDate;
        BaseSalaryAtSigning = contract.BaseSalaryAtSigning;
        Terms = contract.Terms;
    }

    private async Task LoadEmployeeOptionsAsync()
    {
        var employees = await _employeeService.GetAllAsync();
        EmployeeOptions = new ObservableCollection<EmployeeOption>(
            employees.Select(e => new EmployeeOption(e.Id, $"{e.FirstName} {e.LastName} ({e.EmployeeCode})")));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedEmployee is null)
        {
            ErrorMessage = "Select an employee.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            if (_contractId is null)
            {
                var contract = BuildContract();
                await _contractService.CreateAsync(contract);
            }
            else
            {
                var contract = await _contractService.GetByIdAsync(_contractId.Value)
                    ?? throw new InvalidOperationException("This contract no longer exists.");

                ApplyFieldsTo(contract);
                await _contractService.UpdateAsync(contract);
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

    private EmployeeContract BuildContract()
    {
        var contract = new EmployeeContract();
        ApplyFieldsTo(contract);
        return contract;
    }

    private void ApplyFieldsTo(EmployeeContract contract)
    {
        contract.EmployeeId = SelectedEmployee!.Id;
        contract.ContractType = ContractType;
        contract.Status = Status;
        contract.StartDate = StartDate;
        contract.EndDate = EndDate;
        contract.SignedDate = SignedDate;
        contract.BaseSalaryAtSigning = BaseSalaryAtSigning;
        contract.Terms = Terms;
    }
}
