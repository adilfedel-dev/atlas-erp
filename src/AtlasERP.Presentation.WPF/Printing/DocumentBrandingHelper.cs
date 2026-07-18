using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Presentation.WPF.Printing;

/// <summary>
/// Shared branding for printed documents (contracts, payslips, invoices, expense
/// reports): the company's accent color (from Settings) and a logo+name letterhead.
/// </summary>
public static class DocumentBrandingHelper
{
    private static readonly Brush DefaultAccent = Brushes.Black;

    public static Brush GetAccentBrush(Company company)
    {
        if (string.IsNullOrWhiteSpace(company.BrandColorHex))
        {
            return DefaultAccent;
        }

        try
        {
            var color = (Color)ColorConverter.ConvertFromString(company.BrandColorHex)!;
            return new SolidColorBrush(color);
        }
        catch
        {
            return DefaultAccent;
        }
    }

    /// <summary>A logo (if one's been uploaded) beside the legal name, in the brand color — goes at the top of every document.</summary>
    public static Block BuildLetterhead(Company company)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        if (!string.IsNullOrWhiteSpace(company.LogoPath) && File.Exists(company.LogoPath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(company.LogoPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                var image = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    Height = 44,
                    Margin = new Thickness(0, 0, 12, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetColumn(image, 0);
                grid.Children.Add(image);
            }
            catch
            {
                // Missing/corrupt logo file shouldn't block printing — just skip the image.
            }
        }

        var nameBlock = new System.Windows.Controls.TextBlock
        {
            Text = company.LegalName,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = GetAccentBrush(company)
        };
        Grid.SetColumn(nameBlock, 1);
        grid.Children.Add(nameBlock);

        return new BlockUIContainer(grid);
    }
}
