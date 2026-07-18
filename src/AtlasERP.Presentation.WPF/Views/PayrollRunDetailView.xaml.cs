using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class PayrollRunDetailView : Window
{
    public PayrollRunDetailViewModel ViewModel { get; }

    public PayrollRunDetailView(PayrollRunDetailViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
