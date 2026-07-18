using System.Collections.ObjectModel;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Expenses;
using AtlasERP.Core.Domain.Expenses.Enums;
using AtlasERP.Presentation.WPF.Printing;
using AtlasERP.Presentation.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class TravelExpenseDetailViewModel : ObservableObject
{
    private readonly ITravelExpenseService _travelExpenseService;
    private readonly ICompanyContextService _companyContextService;
    private readonly IServiceProvider _serviceProvider;
    private Guid _reportId;
    private TravelExpenseReport? _currentReport;

    [ObservableProperty] private string _employeeName = string.Empty;
    [ObservableProperty] private string _destination = string.Empty;
    [ObservableProperty] private string _purpose = string.Empty;
    [ObservableProperty] private DateTime _departureDate;
    [ObservableProperty] private DateTime _returnDate;
    [ObservableProperty] private TravelExpenseStatus _status;
    [ObservableProperty] private decimal _total;

    [ObservableProperty] private string? _ownerApproverName;
    [ObservableProperty] private DateTime? _ownerApprovedAtUtc;
    [ObservableProperty] private string? _accountantApproverName;
    [ObservableProperty] private DateTime? _accountantApprovedAtUtc;

    [ObservableProperty] private ObservableCollection<TravelExpenseLineItem> _lineItems = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveLineItemCommand))]
    private TravelExpenseLineItem? _selectedLineItem;

    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private DateTime _newLineItemDate = DateTime.Today;
    [ObservableProperty] private ExpenseCategory _newLineItemCategory;
    [ObservableProperty] private string _newLineItemDescription = string.Empty;
    [ObservableProperty] private decimal _newLineItemAmount;

    public IReadOnlyList<ExpenseCategory> CategoryOptions { get; } = Enum.GetValues<ExpenseCategory>();

    public bool IsOwnerApproved => OwnerApprovedAtUtc is not null;
    public bool IsAccountantApproved => AccountantApprovedAtUtc is not null;

    public TravelExpenseDetailViewModel(ITravelExpenseService travelExpenseService, ICompanyContextService companyContextService, IServiceProvider serviceProvider)
    {
        _travelExpenseService = travelExpenseService;
        _companyContextService = companyContextService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync(Guid reportId)
    {
        _reportId = reportId;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var report = await _travelExpenseService.GetByIdAsync(_reportId)
                ?? throw new InvalidOperationException("Travel expense report not found.");
            _currentReport = report;

            EmployeeName = $"{report.Employee?.FirstName} {report.Employee?.LastName}";
            Destination = report.Destination;
            Purpose = report.Purpose;
            DepartureDate = report.DepartureDate;
            ReturnDate = report.ReturnDate;
            Status = report.Status;
            Total = report.Total;
            OwnerApproverName = report.OwnerApproverName;
            OwnerApprovedAtUtc = report.OwnerApprovedAtUtc;
            AccountantApproverName = report.AccountantApproverName;
            AccountantApprovedAtUtc = report.AccountantApprovedAtUtc;
            OnPropertyChanged(nameof(IsOwnerApproved));
            OnPropertyChanged(nameof(IsAccountantApproved));
            LineItems = new ObservableCollection<TravelExpenseLineItem>(report.LineItems.OrderBy(l => l.Date));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load report: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddLineItemAsync()
    {
        if (string.IsNullOrWhiteSpace(NewLineItemDescription) || NewLineItemAmount <= 0)
        {
            ErrorMessage = "Enter a description and a positive amount.";
            return;
        }

        try
        {
            await _travelExpenseService.AddLineItemAsync(_reportId, NewLineItemDate, NewLineItemCategory, NewLineItemDescription, NewLineItemAmount);
            NewLineItemDescription = string.Empty;
            NewLineItemAmount = 0;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not add expense: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelectedLineItem))]
    private async Task RemoveLineItemAsync()
    {
        if (SelectedLineItem is null)
        {
            return;
        }

        try
        {
            await _travelExpenseService.RemoveLineItemAsync(SelectedLineItem.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not remove expense: {ex.Message}";
        }
    }

    private bool HasSelectedLineItem() => SelectedLineItem is not null;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        try
        {
            await _travelExpenseService.SubmitAsync(_reportId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not submit: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RejectAsync()
    {
        try
        {
            await _travelExpenseService.RejectAsync(_reportId);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not reject: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ApproveAsOwnerAsync()
    {
        await CaptureAndApproveAsync("Owner approval", _travelExpenseService.ApproveAsOwnerAsync);
    }

    [RelayCommand]
    private async Task ApproveAsAccountantAsync()
    {
        await CaptureAndApproveAsync("Accountant approval", _travelExpenseService.ApproveAsAccountantAsync);
    }

    private async Task CaptureAndApproveAsync(string roleTitle, Func<Guid, string, string, CancellationToken, Task> approve)
    {
        var signatureViewModel = _serviceProvider.GetRequiredService<SignatureCaptureViewModel>();
        signatureViewModel.RoleTitle = roleTitle;

        var view = _serviceProvider.GetRequiredService<SignatureCaptureView>();
        view.Owner = Application.Current.MainWindow;

        var result = view.ShowDialog();
        if (result == true && view.SavedSignaturePath is not null)
        {
            try
            {
                await approve(_reportId, signatureViewModel.ApproverName, view.SavedSignaturePath, CancellationToken.None);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not record approval: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void Print()
    {
        if (_currentReport is null)
        {
            return;
        }

        var company = _companyContextService.CurrentCompany
            ?? throw new InvalidOperationException("No company selected.");

        var document = TravelExpenseReportDocumentBuilder.Build(_currentReport, company);
        PrintHelper.Print(document, $"Travel Expense Report - {EmployeeName}");
    }
}
