using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class CustomerEditView : Window
{
    public CustomerEditViewModel ViewModel { get; }

    public CustomerEditView(CustomerEditViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.Saved += (_, _) => DialogResult = true;
        viewModel.Cancelled += (_, _) => DialogResult = false;
    }
}
