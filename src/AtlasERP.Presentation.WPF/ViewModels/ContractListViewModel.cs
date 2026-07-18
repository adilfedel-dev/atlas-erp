using System.Collections.ObjectModel;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources;
using AtlasERP.Presentation.WPF.Printing;
using AtlasERP.Presentation.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class ContractListViewModel : ObservableObject
{
    private readonly IContractService _contractService;
    private readonly ICompanyContextService _companyContextService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<EmployeeContract> _contracts = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditContractCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteContractCommand))]
    [NotifyCanExecuteChangedFor(nameof(PrintContractCommand))]
    private EmployeeContract? _selectedContract;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ContractListViewModel(IContractService contractService, ICompanyContextService companyContextService, IServiceProvider serviceProvider)
    {
        _contractService = contractService;
        _companyContextService = companyContextService;
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
            var contracts = await _contractService.GetAllAsync();
            Contracts = new ObservableCollection<EmployeeContract>(contracts);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load contracts: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddContractAsync()
    {
        var view = _serviceProvider.GetRequiredService<ContractEditView>();
        await view.ViewModel.InitializeForCreateAsync();
        ShowEditDialog(view);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task EditContractAsync()
    {
        if (SelectedContract is null)
        {
            return;
        }

        var view = _serviceProvider.GetRequiredService<ContractEditView>();
        await view.ViewModel.InitializeForEditAsync(SelectedContract);
        ShowEditDialog(view);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteContractAsync()
    {
        if (SelectedContract is null)
        {
            return;
        }

        var confirmed = MessageBox.Show(
            $"Delete this contract for {SelectedContract.Employee?.FirstName} {SelectedContract.Employee?.LastName}?",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _contractService.DeleteAsync(SelectedContract.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not delete contract: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void PrintContract()
    {
        if (SelectedContract is null)
        {
            return;
        }

        var company = _companyContextService.CurrentCompany
            ?? throw new InvalidOperationException("No company selected.");

        var document = ContractDocumentBuilder.Build(SelectedContract, company);
        PrintHelper.Print(document, $"Contract - {SelectedContract.Employee?.FirstName} {SelectedContract.Employee?.LastName}");
    }

    private bool HasSelection() => SelectedContract is not null;

    private void ShowEditDialog(ContractEditView view)
    {
        view.Owner = Application.Current.MainWindow;
        var saved = view.ShowDialog();
        if (saved == true)
        {
            _ = LoadAsync();
        }
    }
}
