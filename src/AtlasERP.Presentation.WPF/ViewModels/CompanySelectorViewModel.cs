using System.Collections.ObjectModel;
using AtlasERP.Core.Domain.Master;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class CompanySelectorViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Company> _companies = new();

    [ObservableProperty]
    private Company? _selectedCompany;

    public event EventHandler<Company>? CompanySelected;

    public void Initialize(IEnumerable<Company> companies)
    {
        Companies = new ObservableCollection<Company>(companies);
        SelectedCompany = Companies.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanConfirmSelection))]
    private void ConfirmSelection()
    {
        if (SelectedCompany is not null)
        {
            CompanySelected?.Invoke(this, SelectedCompany);
        }
    }

    private bool CanConfirmSelection() => SelectedCompany is not null;
}
