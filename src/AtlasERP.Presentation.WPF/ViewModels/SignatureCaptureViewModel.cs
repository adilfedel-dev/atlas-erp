using CommunityToolkit.Mvvm.ComponentModel;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class SignatureCaptureViewModel : ObservableObject
{
    [ObservableProperty]
    private string _roleTitle = "Signature";

    [ObservableProperty]
    private string _approverName = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;
}
