using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AtlasERP.Presentation.WPF.ViewModels;

namespace AtlasERP.Presentation.WPF.Views;

public partial class SignatureCaptureView : Window
{
    public SignatureCaptureViewModel ViewModel { get; }

    /// <summary>Set once Save succeeds — the caller reads this after ShowDialog() returns true.</summary>
    public string? SavedSignaturePath { get; private set; }

    public SignatureCaptureView(SignatureCaptureViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        SignatureInkCanvas.Strokes.Clear();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.ApproverName))
        {
            ViewModel.ErrorMessage = "Enter the signer's name.";
            return;
        }

        if (SignatureInkCanvas.Strokes.Count == 0)
        {
            ViewModel.ErrorMessage = "Draw a signature before saving.";
            return;
        }

        try
        {
            var signaturesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Signatures");
            Directory.CreateDirectory(signaturesDirectory);
            var filePath = Path.Combine(signaturesDirectory, $"{Guid.NewGuid()}.png");

            var renderBitmap = new RenderTargetBitmap(
                Math.Max(1, (int)SignatureInkCanvas.ActualWidth),
                Math.Max(1, (int)SignatureInkCanvas.ActualHeight),
                96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(SignatureInkCanvas);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using (var stream = File.Create(filePath))
            {
                encoder.Save(stream);
            }

            SavedSignaturePath = filePath;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ViewModel.ErrorMessage = $"Could not save signature: {ex.Message}";
        }
    }
}
