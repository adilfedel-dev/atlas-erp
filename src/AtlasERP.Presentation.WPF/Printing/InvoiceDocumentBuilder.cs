using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using AtlasERP.Core.Domain.Master;
using AtlasERP.Core.Domain.Sales;

namespace AtlasERP.Presentation.WPF.Printing;

public static class InvoiceDocumentBuilder
{
    public static FlowDocument Build(Invoice invoice, Company company)
    {
        var customer = invoice.Customer
            ?? throw new InvalidOperationException("Invoice must have its Customer loaded before printing.");

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12
        };

        document.Blocks.Add(new Paragraph(new Run("INVOICE"))
        {
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 2)
        });
        document.Blocks.Add(new Paragraph(new Run(invoice.InvoiceNumber))
        {
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 24)
        });

        var headerTable = new Table { CellSpacing = 0 };
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var headerRowGroup = new TableRowGroup();
        headerTable.RowGroups.Add(headerRowGroup);

        var headerRow = new TableRow();
        var fromCell = new TableCell();
        fromCell.Blocks.Add(new Paragraph(new Run("From")) { FontWeight = FontWeights.SemiBold });
        fromCell.Blocks.Add(new Paragraph(new Run(company.LegalName)));
        headerRow.Cells.Add(fromCell);

        var toCell = new TableCell();
        toCell.Blocks.Add(new Paragraph(new Run("Bill to")) { FontWeight = FontWeights.SemiBold });
        toCell.Blocks.Add(new Paragraph(new Run(customer.Name)));
        if (!string.IsNullOrWhiteSpace(customer.Address))
        {
            toCell.Blocks.Add(new Paragraph(new Run(customer.Address)));
        }
        headerRow.Cells.Add(toCell);
        headerRowGroup.Rows.Add(headerRow);
        document.Blocks.Add(headerTable);

        var datesTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 16, 0, 24) };
        datesTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
        datesTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var datesRowGroup = new TableRowGroup();
        datesTable.RowGroups.Add(datesRowGroup);
        datesRowGroup.Rows.Add(BuildLabelValueRow("Issue date", invoice.IssueDate.ToString("d")));
        datesRowGroup.Rows.Add(BuildLabelValueRow("Due date", invoice.DueDate.ToString("d")));
        datesRowGroup.Rows.Add(BuildLabelValueRow("Status", invoice.Status.ToString()));
        document.Blocks.Add(datesTable);

        var lineItemsTable = new Table { CellSpacing = 0 };
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
        lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(110) });
        var lineItemsRowGroup = new TableRowGroup();
        lineItemsTable.RowGroups.Add(lineItemsRowGroup);

        var headerCellRow = new TableRow();
        headerCellRow.Cells.Add(new TableCell(new Paragraph(new Run("Description")) { FontWeight = FontWeights.SemiBold }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 0, 0, 4) });
        headerCellRow.Cells.Add(new TableCell(new Paragraph(new Run("Qty")) { FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Right }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 0, 0, 4) });
        headerCellRow.Cells.Add(new TableCell(new Paragraph(new Run("Unit price")) { FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Right }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 0, 0, 4) });
        headerCellRow.Cells.Add(new TableCell(new Paragraph(new Run("Amount")) { FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Right }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 0, 0, 4) });
        lineItemsRowGroup.Rows.Add(headerCellRow);

        foreach (var lineItem in invoice.LineItems)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Description))) { Padding = new Thickness(0, 4, 0, 4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Quantity.ToString("0.##"))) { TextAlignment = TextAlignment.Right }) { Padding = new Thickness(0, 4, 0, 4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.UnitPrice.ToString("C"))) { TextAlignment = TextAlignment.Right }) { Padding = new Thickness(0, 4, 0, 4) });
            row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.LineTotal.ToString("C"))) { TextAlignment = TextAlignment.Right }) { Padding = new Thickness(0, 4, 0, 4) });
            lineItemsRowGroup.Rows.Add(row);
        }

        document.Blocks.Add(lineItemsTable);

        var summaryTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 20, 0, 0) };
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
        var summaryRowGroup = new TableRowGroup();
        summaryTable.RowGroups.Add(summaryRowGroup);

        summaryRowGroup.Rows.Add(BuildLabelValueRow("Subtotal", invoice.Subtotal.ToString("C")));
        summaryRowGroup.Rows.Add(BuildLabelValueRow($"Tax ({invoice.TaxRatePercent:0.##}%)", invoice.TaxAmount.ToString("C")));

        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(new Paragraph(new Run("TOTAL")) { FontSize = 15, FontWeight = FontWeights.Bold })
        {
            Padding = new Thickness(0, 10, 0, 0),
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 1, 0, 0)
        });
        totalRow.Cells.Add(new TableCell(new Paragraph(new Run(invoice.Total.ToString("C"))) { FontSize = 15, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right })
        {
            Padding = new Thickness(0, 10, 0, 0),
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 1, 0, 0)
        });
        summaryRowGroup.Rows.Add(totalRow);

        var amountPaid = invoice.Receipts.Sum(r => r.Amount);
        summaryRowGroup.Rows.Add(BuildLabelValueRow("Amount paid", amountPaid.ToString("C")));
        summaryRowGroup.Rows.Add(BuildLabelValueRow("Balance due", (invoice.Total - amountPaid).ToString("C")));

        document.Blocks.Add(summaryTable);

        if (!string.IsNullOrWhiteSpace(invoice.Notes))
        {
            document.Blocks.Add(new Paragraph(new Run("Notes")) { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 24, 0, 4) });
            document.Blocks.Add(new Paragraph(new Run(invoice.Notes)));
        }

        return document;
    }

    private static TableRow BuildLabelValueRow(string label, string value)
    {
        var row = new TableRow();
        row.Cells.Add(new TableCell(new Paragraph(new Run(label)))
        {
            Padding = new Thickness(0, 3, 0, 3)
        });
        row.Cells.Add(new TableCell(new Paragraph(new Run(value)) { TextAlignment = TextAlignment.Right })
        {
            Padding = new Thickness(0, 3, 0, 3)
        });
        return row;
    }
}
