using System.Windows;
using System.Windows.Input;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF;

public partial class MainWindow : Window
{
    private Rect _restoreBounds;
    private bool _isMaximized;

    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        Closed += (_, _) => ViewModel.Dispose();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeRestore_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    /// <summary>Resizes to the work area (screen minus taskbar) instead of using
    /// WindowState.Maximized, which covers the taskbar on a WindowStyle="None" window.</summary>
    private void ToggleMaximize()
    {
        if (_isMaximized)
        {
            Left = _restoreBounds.Left;
            Top = _restoreBounds.Top;
            Width = _restoreBounds.Width;
            Height = _restoreBounds.Height;
            _isMaximized = false;
            MaximizeRestoreButton.Content = "";
        }
        else
        {
            _restoreBounds = new Rect(Left, Top, Width, Height);
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left;
            Top = workArea.Top;
            Width = workArea.Width;
            Height = workArea.Height;
            _isMaximized = true;
            MaximizeRestoreButton.Content = "";
        }
    }
}
