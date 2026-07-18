using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using AtlasERP.Core.Domain.Master;
using AtlasERP.Core.Domain.Payroll;
using AtlasERP.Core.Domain.Payroll.Enums;

namespace AtlasERP.Presentation.WPF.Printing;

public static class PayslipDocumentBuilder
{
    public static FlowDocument BuildSingle(Payslip payslip, string periodLabel, Company company)
    {
        var document = NewDocument();
        document.Blocks.Add(BuildSection(payslip, periodLabel, company, breakPageBefore: false));
        return document;
    }

    /// <summary>One payslip per page, in one print job — the "print all payslips for this run" action.</summary>
    public static FlowDocument BuildAll(IEnumerable<Payslip> payslips, string periodLabel, Company company)
    {
        var document = NewDocument();
        var isFirst = true;
        foreach (var payslip in payslips)
        {
            document.Blocks.Add(BuildSection(payslip, periodLabel, company, breakPageBefore: !isFirst));
            isFirst = false;
        }

        return document;
    }

    private static FlowDocument NewDocument() => new()
    {
        FontFamily = new FontFamily("Segoe UI"),
        FontSize = 12
    };

    private static Section BuildSection(Payslip payslip, string periodLabel, Company company, bool breakPageBefore)
    {
        var employee = payslip.Employee
            ?? throw new InvalidOperationException("Payslip must have its Employee loaded before printing.");

        var accent = DocumentBrandingHelper.GetAccentBrush(company);
        var section = new Section { BreakPageBefore = breakPageBefore };

        section.Blocks.Add(DocumentBrandingHelper.BuildLetterhead(company));

        section.Blocks.Add(new Paragraph(new Run("Payslip"))
        {
            FontFamily = DocumentBrandingHelper.HeadingFontFamily,
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 2)
        });
        section.Blocks.Add(new Paragraph(new Run(periodLabel))
        {
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 24)
        });

        var detailsTable = new Table { CellSpacing = 0 };
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(170) });
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(546) });
        var detailsRowGroup = new TableRowGroup();
        detailsTable.RowGroups.Add(detailsRowGroup);

        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Employee", $"{employee.FirstName} {employee.LastName} ({employee.EmployeeCode})"));
        detailsRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Job title", employee.JobTitle));
        section.Blocks.Add(detailsTable);

        if (payslip.LineItems.Count > 0)
        {
            section.Blocks.Add(new Paragraph(new Run("Adjustments"))
            {
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 20, 0, 4)
            });

            var lineItemsTable = new Table { CellSpacing = 0 };
            lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(496) });
            lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            lineItemsTable.Columns.Add(new TableColumn { Width = new GridLength(120) });
            var lineItemsRowGroup = new TableRowGroup();
            lineItemsTable.RowGroups.Add(lineItemsRowGroup);

            foreach (var lineItem in payslip.LineItems)
            {
                var sign = lineItem.Type == PayslipLineItemType.Deduction ? "-" : "+";
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Description))) { Padding = new Thickness(0, 3, 0, 3) });
                row.Cells.Add(new TableCell(new Paragraph(new Run(lineItem.Type.ToString()))) { Padding = new Thickness(0, 3, 0, 3) });
                row.Cells.Add(new TableCell(new Paragraph(new Run($"{sign}{lineItem.Amount:C}")) { TextAlignment = TextAlignment.Right }) { Padding = new Thickness(0, 3, 0, 3) });
                lineItemsRowGroup.Rows.Add(row);
            }

            section.Blocks.Add(lineItemsTable);
        }

        var summaryTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 24, 0, 0) };
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(566) });
        summaryTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
        var summaryRowGroup = new TableRowGroup();
        summaryTable.RowGroups.Add(summaryRowGroup);

        summaryRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Gross pay", payslip.GrossPay.ToString("C"), rightAlignValue: true));
        summaryRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Total bonuses", $"+{payslip.TotalBonuses:C}", rightAlignValue: true));
        summaryRowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Total deductions", $"-{payslip.TotalDeductions:C}", rightAlignValue: true));

        var netRow = new TableRow();
        netRow.Cells.Add(new TableCell(new Paragraph(new Run("NET PAY")) { FontSize = 15, FontWeight = FontWeights.Bold, Foreground = accent })
        {
            Padding = new Thickness(0, 10, 0, 0),
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 1, 0, 0)
        });
        netRow.Cells.Add(new TableCell(new Paragraph(new Run(payslip.NetPay.ToString("C"))) { FontSize = 15, FontWeight = FontWeights.Bold, Foreground = accent, TextAlignment = TextAlignment.Right })
        {
            Padding = new Thickness(0, 10, 0, 0),
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 1, 0, 0)
        });
        summaryRowGroup.Rows.Add(netRow);

        section.Blocks.Add(summaryTable);

        return section;
    }
}
