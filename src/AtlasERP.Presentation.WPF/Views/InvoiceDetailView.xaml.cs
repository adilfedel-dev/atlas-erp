using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class InvoiceDetailView : Window
{
    public InvoiceDetailViewModel ViewModel { get; }

    public InvoiceDetailView(InvoiceDetailViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
