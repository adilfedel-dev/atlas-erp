using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class GenerateRunViewModel : ObservableObject
{
    [ObservableProperty]
    private int _year = DateTime.Today.Year;

    [ObservableProperty]
    private int _month = DateTime.Today.Month;

    [ObservableProperty]
    private string? _errorMessage;

    public IReadOnlyList<int> MonthOptions { get; } = Enumerable.Range(1, 12).ToList();

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    [RelayCommand]
    private void Confirm()
    {
        if (Month is < 1 or > 12)
        {
            ErrorMessage = "Month must be between 1 and 12.";
            return;
        }

        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
