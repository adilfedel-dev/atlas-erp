using System.Windows;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class GenerateRunView : Window
{
    public GenerateRunViewModel ViewModel { get; }

    public GenerateRunView(GenerateRunViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
}
