using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class TravelExpenseCreateView : Window
{
    public TravelExpenseCreateViewModel ViewModel { get; }

    public TravelExpenseCreateView(TravelExpenseCreateViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
