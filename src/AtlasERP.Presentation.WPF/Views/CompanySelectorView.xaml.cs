using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class CompanySelectorView : Window
{
    public CompanySelectorViewModel ViewModel { get; }

    public CompanySelectorView(CompanySelectorViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
