using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AtlasERP.Core.Domain.Expenses;
using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Presentation.WPF.Printing;

public static class TravelExpenseReportDocumentBuilder
{
    public static FlowDocument Build(TravelExpenseReport report, Company company)
    {
        var employee = report.Employee
            ?? throw new InvalidOperationException("Report must have its Employee loaded before printing.");

        var accent = DocumentBrandingHelper.GetAccentBrush(company);

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12
        };

        document.Blocks.Add(DocumentBrandingHelper.BuildLetterhead(company));

        document.Blocks.Add(new Paragraph(new Run("Travel Expense Report"))
        {
            FontFamily = DocumentBrandingHelper.HeadingFontFamily,
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 24)
        });

        var detailsTable = new Table { CellSpacing = 0 };
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(170) });
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(500) });
        var detailsRowGroup = new TableRowGroup();
        detailsTable.RowGroups.Add(detailsRowGroup);

        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Employee", $"{employee.FirstName} {employee.LastName} ({employee.EmployeeCode})"));
        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Destination", report.Destination));
        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Purpose", report.Purpose));
        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Departure", report.DepartureDate.ToString("d")));
        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Return", report.ReturnDate.ToString("d")));
        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Status", report.Status.ToString()));

        document.Blocks.Add(detailsTable);

        document.Blocks.Add(new Paragraph(new Run("Expenses"))
        {
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 24, 0, 4)
        });

        var lineItemsTable = new Table { CellSpacing = 0 };
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(90) });
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(380) });
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
        var lineItemsRowGroup = new TableRowGroup();
        lineItemsTable.RowGroups.Add(lineItemsRowGroup);

        var headerRow = new TableRow();
        headerRow.Cells.Add(HeaderCell("Date", accent));
        headerRow.Cells.Add(HeaderCell("Category", accent));
        headerRow.Cells.Add(HeaderCell("Description", accent));
        headerRow.Cells.Add(HeaderCell("Amount", accent, TextAlignment.Right));
        lineItemsRowGroup.Rows.Add(headerRow);

        foreach (var lineItem in report.LineItems.OrderBy(l => l.Date))
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Date.ToString("d")))) { Padding = new Thickness(0, 4, 0, 4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Category.ToString()))) { Padding = new Thickness(0, 4, 0, 4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Description))) { Padding = new Thickness(0, 4, 0, 4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Amount.ToString("C"))) { TextAlignment = TextAlignment.Right }) { Padding = new Thickness(0, 4, 0, 4) });
            lineItemsRowGroup.Rows.Add(row);
        }

        document.Blocks.Add(lineItemsTable);

        var totalTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 16, 0, 0) };
        totalTable.Columns.Add(new TableColumn { Width = new GridLength(520) });
        totalTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
        var totalRowGroup = new TableRowGroup();
        totalTable.RowGroups.Add(totalRowGroup);

        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(new Paragraph(new Run("TOTAL")) { FontSize = 15, FontWeight = FontWeights.Bold, Foreground = accent })
        {
            Padding = new Thickness(0, 10, 0, 0),
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 1, 0, 0)
        });
        totalRow.Cells.Add(new TableCell(new Paragraph(new Run(report.Total.ToString("C"))) { FontSize = 15, FontWeight = FontWeights.Bold, Foreground = accent, TextAlignment = TextAlignment.Right })
        {
            Padding = new Thickness(0, 10, 0, 0),
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 1, 0, 0)
        });
        totalRowGroup.Rows.Add(totalRow);
        document.Blocks.Add(totalTable);

        var signatureTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 50, 0, 0) };
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(330) });
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(40) });
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(330) });
        var signatureRowGroup = new TableRowGroup();
        signatureTable.RowGroups.Add(signatureRowGroup);

        var signatureRow = new TableRow();
        signatureRow.Cells.Add(BuildSignatureCell("Owner approval", report.OwnerApproverName, report.OwnerSignaturePath, report.OwnerApprovedAtUtc, accent));
        signatureRow.Cells.Add(new TableCell(new Paragraph()));
        signatureRow.Cells.Add(BuildSignatureCell("Accountant approval", report.AccountantApproverName, report.AccountantSignaturePath, report.AccountantApprovedAtUtc, accent));
        signatureRowGroup.Rows.Add(signatureRow);

        document.Blocks.Add(signatureTable);

        return document;
    }

    private static TableCell HeaderCell(string text, Brush accent, TextAlignment alignment = TextAlignment.Left) =>
        new(new Paragraph(new Run(text)) { FontWeight = FontWeights.SemiBold, TextAlignment = alignment })
        {
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 4)
        };

    private static TableCell BuildSignatureCell(string role, string? approverName, string? signaturePath, DateTime? approvedAtUtc, Brush accent)
    {
        var cell = new TableCell();

        if (approvedAtUtc is not null && !string.IsNullOrWhiteSpace(signaturePath) && File.Exists(signaturePath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(signaturePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                var image = new System.Windows.Controls.Image
                {
                    Source = bitmap,
                    Height = 60,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                var container = new BlockUIContainer(image) { Margin = new Thickness(0, 0, 0, 4) };
                cell.Blocks.Add(container);
            }
            catch
            {
                // Missing/corrupt signature image shouldn't block printing.
            }
        }
        else
        {
            cell.Blocks.Add(new Paragraph { Margin = new Thickness(0, 0, 0, 60) });
        }

        cell.BorderBrush = accent;
        cell.BorderThickness = new Thickness(0, 0, 0, 1);
        cell.Blocks.Add(new Paragraph(new Run(role)) { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 0) });
        cell.Blocks.Add(new Paragraph(new Run(approverName ?? "Not yet signed")));
        if (approvedAtUtc is not null)
        {
            cell.Blocks.Add(new Paragraph(new Run(approvedAtUtc.Value.ToLocalTime().ToString("d MMM yyyy, HH:mm"))) { Foreground = Brushes.Gray, FontSize = 10 });
        }

        return cell;
    }
}
