using System.Collections.ObjectModel;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Payroll;
using AtlasERP.Core.Domain.Payroll.Enums;
using AtlasERP.Presentation.WPF.Printing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class PayrollRunDetailViewModel : ObservableObject
{
    private readonly IPayrollService _payrollService;
    private readonly ICompanyContextService _companyContextService;
    private Guid _runId;

    [ObservableProperty]
    private string _periodLabel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDraft))]
    private PayrollRunStatus _status;

    [ObservableProperty]
    private ObservableCollection<Payslip> _payslips = new();

    [ObservableProperty]
    private Payslip? _selectedPayslip;

    [ObservableProperty]
    private ObservableCollection<PayslipLineItem> _selectedPayslipLineItems = new();

    [ObservableProperty]
    private PayslipLineItem? _selectedLineItem;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private PayslipLineItemType _newLineItemType = PayslipLineItemType.Bonus;

    [ObservableProperty]
    private string _newLineItemDescription = string.Empty;

    [ObservableProperty]
    private decimal _newLineItemAmount;

    public IReadOnlyList<PayslipLineItemType> LineItemTypeOptions { get; } = Enum.GetValues<PayslipLineItemType>();

    public bool IsDraft => Status == PayrollRunStatus.Draft;

    public PayrollRunDetailViewModel(IPayrollService payrollService, ICompanyContextService companyContextService)
    {
        _payrollService = payrollService;
        _companyContextService = companyContextService;
    }

    public async Task InitializeAsync(Guid runId)
    {
        _runId = runId;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var run = await _payrollService.GetRunByIdAsync(_runId)
                ?? throw new InvalidOperationException("Payroll run not found.");

            PeriodLabel = new DateTime(run.PeriodYear, run.PeriodMonth, 1).ToString("MMMM yyyy");
            Status = run.Status;
            Payslips = new ObservableCollection<Payslip>(run.Payslips.OrderBy(p => p.Employee?.LastName));
            SelectedPayslip = Payslips.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load payroll run: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedPayslipChanged(Payslip? value)
    {
        SelectedPayslipLineItems = value is null
            ? new ObservableCollection<PayslipLineItem>()
            : new ObservableCollection<PayslipLineItem>(value.LineItems);
    }

    [RelayCommand]
    private async Task AddLineItemAsync()
    {
        if (SelectedPayslip is null)
        {
            ErrorMessage = "Select a payslip first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewLineItemDescription) || NewLineItemAmount <= 0)
        {
            ErrorMessage = "Enter a description and a positive amount.";
            return;
        }

        try
        {
            await _payrollService.AddLineItemAsync(SelectedPayslip.Id, NewLineItemType, NewLineItemDescription, NewLineItemAmount);
            NewLineItemDescription = string.Empty;
            NewLineItemAmount = 0;
            var selectedEmployeeId = SelectedPayslip.EmployeeId;
            await LoadAsync();
            SelectedPayslip = Payslips.FirstOrDefault(p => p.EmployeeId == selectedEmployeeId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not add line item: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RemoveLineItemAsync()
    {
        if (SelectedLineItem is null)
        {
            return;
        }

        try
        {
            var selectedEmployeeId = SelectedPayslip?.EmployeeId;
            await _payrollService.RemoveLineItemAsync(SelectedLineItem.Id);
            await LoadAsync();
            if (selectedEmployeeId is not null)
            {
                SelectedPayslip = Payslips.FirstOrDefault(p => p.EmployeeId == selectedEmployeeId);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not remove line item: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task FinalizeRunAsync()
    {
        try
        {
            await _payrollService.FinalizeRunAsync(_runId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not finalize run: {ex.Message}";
        }
    }

    [RelayCommand]
    private void PrintSelectedPayslip()
    {
        if (SelectedPayslip is null)
        {
            return;
        }

        var company = _companyContextService.CurrentCompany
            ?? throw new InvalidOperationException("No company selected.");

        var document = PayslipDocumentBuilder.BuildSingle(SelectedPayslip, PeriodLabel, company);
        PrintHelper.Print(document, $"Payslip - {SelectedPayslip.Employee?.FirstName} {SelectedPayslip.Employee?.LastName}");
    }

    [RelayCommand]
    private void PrintAllPayslips()
    {
        if (Payslips.Count == 0)
        {
            return;
        }

        var company = _companyContextService.CurrentCompany
            ?? throw new InvalidOperationException("No company selected.");

        var document = PayslipDocumentBuilder.BuildAll(Payslips, PeriodLabel, company);
        PrintHelper.Print(document, $"Payslips - {PeriodLabel}");
    }
}
