using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class InvoiceCreateView : Window
{
    public InvoiceCreateViewModel ViewModel { get; }

    public InvoiceCreateView(InvoiceCreateViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
