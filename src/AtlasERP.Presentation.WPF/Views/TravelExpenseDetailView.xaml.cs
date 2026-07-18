using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class TravelExpenseDetailView : Window
{
    public TravelExpenseDetailViewModel ViewModel { get; }

    public TravelExpenseDetailView(TravelExpenseDetailViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
