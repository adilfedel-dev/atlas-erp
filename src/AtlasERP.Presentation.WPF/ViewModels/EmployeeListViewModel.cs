using System.Collections.ObjectModel;
using System.Windows;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources;
using AtlasERP.Presentation.WPF.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class EmployeeListViewModel : ObservableObject
{
    private readonly IEmployeeService _employeeService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditEmployeeCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteEmployeeCommand))]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public EmployeeListViewModel(IEmployeeService employeeService, IServiceProvider serviceProvider)
    {
        _employeeService = employeeService;
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
            var employees = await _employeeService.GetAllAsync();
            Employees = new ObservableCollection<Employee>(employees);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load employees: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddEmployee()
    {
        var view = _serviceProvider.GetRequiredService<EmployeeEditView>();
        view.ViewModel.InitializeForCreate();
        ShowEditDialog(view);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void EditEmployee()
    {
        if (SelectedEmployee is null)
        {
            return;
        }

        var view = _serviceProvider.GetRequiredService<EmployeeEditView>();
        view.ViewModel.InitializeForEdit(SelectedEmployee);
        ShowEditDialog(view);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteEmployeeAsync()
    {
        if (SelectedEmployee is null)
        {
            return;
        }

        var confirmed = MessageBox.Show(
            $"Delete {SelectedEmployee.FirstName} {SelectedEmployee.LastName}? This cannot be undone.",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmed != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _employeeService.DeleteAsync(SelectedEmployee.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not delete employee: {ex.Message}";
        }
    }

    private bool HasSelection() => SelectedEmployee is not null;

    private void ShowEditDialog(EmployeeEditView view)
    {
        view.Owner = Application.Current.MainWindow;
        var saved = view.ShowDialog();
        if (saved == true)
        {
            _ = LoadAsync();
        }
    }
}
