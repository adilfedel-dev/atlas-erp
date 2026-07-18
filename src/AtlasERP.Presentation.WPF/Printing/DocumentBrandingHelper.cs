using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Presentation.WPF.Printing;

/// <summary>
/// Shared branding for printed documents (contracts, payslips, invoices, expense
/// reports): the company's accent color (from Settings), a logo+name letterhead with a
/// colored rule beneath it, and consistent label/value row styling.
/// </summary>
public static class DocumentBrandingHelper
{
    private static readonly Brush DefaultAccent = Brushes.Black;
    private static readonly Brush MutedLabelBrush = new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80));

    /// <summary>Serif for titles/headings — pairs with the Segoe UI body text for a more
    /// premium letterhead feel than an all-sans-serif document.</summary>
    public static readonly FontFamily HeadingFontFamily = new("Georgia");

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

    /// <summary>Logo (if one's been uploaded) beside the legal name, with a colored rule
    /// underneath — goes at the top of every document.</summary>
    public static Block BuildLetterhead(Company company)
    {
        var accent = GetAccentBrush(company);

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

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

                var image = new Image
                {
                    Source = bitmap,
                    Height = 48,
                    Margin = new Thickness(0, 0, 14, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetColumn(image, 0);
                headerGrid.Children.Add(image);
            }
            catch
            {
                // Missing/corrupt logo file shouldn't block printing — just skip the image.
            }
        }

        var nameBlock = new TextBlock
        {
            Text = company.LegalName,
            FontFamily = HeadingFontFamily,
            FontSize = 19,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Black
        };
        Grid.SetColumn(nameBlock, 1);
        headerGrid.Children.Add(nameBlock);

        var rule = new Border
        {
            Height = 3,
            Background = accent,
            Margin = new Thickness(0, 14, 0, 24)
        };

        var stack = new StackPanel();
        stack.Children.Add(headerGrid);
        stack.Children.Add(rule);

        return new BlockUIContainer(stack);
    }

    /// <summary>A quiet closing rule + company name/generated-date line — goes at the
    /// bottom of every document so the whole family reads as one consistent product.</summary>
    public static Block BuildFooter(Company company)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 40, 0, 0) };

        stack.Children.Add(new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
            Margin = new Thickness(0, 0, 0, 10)
        });

        var footerGrid = new Grid();
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var companyText = new TextBlock
        {
            Text = company.LegalName,
            FontSize = 9,
            Foreground = Brushes.Gray
        };
        Grid.SetColumn(companyText, 0);
        footerGrid.Children.Add(companyText);

        var dateText = new TextBlock
        {
            Text = $"Generated {DateTime.Now:d MMM yyyy}",
            FontSize = 9,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(dateText, 1);
        footerGrid.Children.Add(dateText);

        stack.Children.Add(footerGrid);

        return new BlockUIContainer(stack);
    }

    /// <summary>A small-caps-style muted label over a plain value — the row styling used
    /// throughout every printed document for consistency.</summary>
    public static TableRow BuildLabelValueRow(string label, string value, Brush? valueForeground = null, bool rightAlignValue = false)
    {
        var row = new TableRow();
        row.Cells.Add(new TableCell(new Paragraph(new Run(label.ToUpper(CultureInfo.CurrentCulture)))
        {
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            Foreground = MutedLabelBrush
        })
        {
            Padding = new Thickness(0, 5, 0, 5)
        });
        row.Cells.Add(new TableCell(new Paragraph(new Run(value))
        {
            Foreground = valueForeground ?? Brushes.Black,
            TextAlignment = rightAlignValue ? TextAlignment.Right : TextAlignment.Left
        })
        {
            Padding = new Thickness(0, 5, 0, 5)
        });
        return row;
    }
}
