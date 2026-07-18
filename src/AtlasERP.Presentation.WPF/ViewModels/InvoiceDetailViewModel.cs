using System.Collections.ObjectModel;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Sales;
using AtlasERP.Core.Domain.Sales.Enums;
using AtlasERP.Presentation.WPF.Printing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class InvoiceDetailViewModel : ObservableObject
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICompanyContextService _companyContextService;
    private Guid _invoiceId;
    private Invoice? _currentInvoice;

    [ObservableProperty] private string _invoiceNumber = string.Empty;
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private DateTime _issueDate;
    [ObservableProperty] private DateTime _dueDate;
    [ObservableProperty] private InvoiceStatus _status;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _taxAmount;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private decimal _amountPaid;
    [ObservableProperty] private decimal _balance;

    [ObservableProperty] private ObservableCollection<InvoiceLineItem> _lineItems = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveLineItemCommand))]
    private InvoiceLineItem? _selectedLineItem;

    [ObservableProperty] private ObservableCollection<Receipt> _receipts = new();

    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private string _newLineItemDescription = string.Empty;
    [ObservableProperty] private decimal _newLineItemQuantity = 1;
    [ObservableProperty] private decimal _newLineItemUnitPrice;

    [ObservableProperty] private decimal _newReceiptAmount;
    [ObservableProperty] private DateTime _newReceiptDate = DateTime.Today;
    [ObservableProperty] private PaymentMethod _newReceiptMethod;

    public IReadOnlyList<InvoiceStatus> StatusOptions { get; } = Enum.GetValues<InvoiceStatus>();
    public IReadOnlyList<PaymentMethod> PaymentMethodOptions { get; } = Enum.GetValues<PaymentMethod>();

    public InvoiceDetailViewModel(IInvoiceService invoiceService, ICompanyContextService companyContextService)
    {
        _invoiceService = invoiceService;
        _companyContextService = companyContextService;
    }

    public async Task InitializeAsync(Guid invoiceId)
    {
        _invoiceId = invoiceId;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var invoice = await _invoiceService.GetByIdAsync(_invoiceId)
                ?? throw new InvalidOperationException("Invoice not found.");
            _currentInvoice = invoice;

            InvoiceNumber = invoice.InvoiceNumber;
            CustomerName = invoice.Customer?.Name ?? string.Empty;
            IssueDate = invoice.IssueDate;
            DueDate = invoice.DueDate;
            Status = invoice.Status;
            Subtotal = invoice.Subtotal;
            TaxAmount = invoice.TaxAmount;
            Total = invoice.Total;
            AmountPaid = invoice.Receipts.Sum(r => r.Amount);
            Balance = invoice.Total - AmountPaid;
            LineItems = new ObservableCollection<InvoiceLineItem>(invoice.LineItems);
            Receipts = new ObservableCollection<Receipt>(invoice.Receipts.OrderByDescending(r => r.ReceivedDate));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load invoice: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddLineItemAsync()
    {
        if (string.IsNullOrWhiteSpace(NewLineItemDescription) || NewLineItemQuantity <= 0 || NewLineItemUnitPrice < 0)
        {
            ErrorMessage = "Enter a description, a positive quantity, and a unit price.";
            return;
        }

        try
        {
            await _invoiceService.AddLineItemAsync(_invoiceId, NewLineItemDescription, NewLineItemQuantity, NewLineItemUnitPrice);
            NewLineItemDescription = string.Empty;
            NewLineItemQuantity = 1;
            NewLineItemUnitPrice = 0;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not add line item: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveLineItem))]
    private async Task RemoveLineItemAsync()
    {
        if (SelectedLineItem is null)
        {
            return;
        }

        try
        {
            await _invoiceService.RemoveLineItemAsync(SelectedLineItem.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not remove line item: {ex.Message}";
        }
    }

    private bool CanRemoveLineItem() => SelectedLineItem is not null;

    [RelayCommand]
    private async Task RecordReceiptAsync()
    {
        if (NewReceiptAmount <= 0)
        {
            ErrorMessage = "Enter a positive payment amount.";
            return;
        }

        try
        {
            await _invoiceService.RecordReceiptAsync(_invoiceId, NewReceiptAmount, NewReceiptDate, NewReceiptMethod, null);
            NewReceiptAmount = 0;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not record payment: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateStatusAsync()
    {
        try
        {
            await _invoiceService.UpdateStatusAsync(_invoiceId, Status);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not update status: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Print()
    {
        if (_currentInvoice is null)
        {
            return;
        }

        var company = _companyContextService.CurrentCompany
            ?? throw new InvalidOperationException("No company selected.");

        var document = InvoiceDocumentBuilder.Build(_currentInvoice, company);
        PrintHelper.Print(document, $"Invoice {_currentInvoice.InvoiceNumber}");
    }
}
