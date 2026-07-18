using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using AtlasERP.Core.Domain.HumanResources;
using AtlasERP.Core.Domain.Master;

namespace AtlasERP.Presentation.WPF.Printing;

public static class ContractDocumentBuilder
{
    public static FlowDocument Build(EmployeeContract contract, Company company)
    {
        var employee = contract.Employee
            ?? throw new InvalidOperationException("Contract must have its Employee loaded before printing.");

        var accent = DocumentBrandingHelper.GetAccentBrush(company);

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 13
        };

        document.Blocks.Add(DocumentBrandingHelper.BuildLetterhead(company));

        document.Blocks.Add(new Paragraph(new Run("Employment Contract"))
        {
            FontFamily = DocumentBrandingHelper.HeadingFontFamily,
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 28)
        });

        var detailsTable = new Table { CellSpacing = 0 };
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(170) });
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(500) });
        var rowGroup = new TableRowGroup();
        detailsTable.RowGroups.Add(rowGroup);

        rowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Employee", $"{employee.FirstName} {employee.LastName} ({employee.EmployeeCode})"));
        rowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Job title", employee.JobTitle));
        rowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Contract type", contract.ContractType.ToString()));
        rowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Start date", contract.StartDate.ToString("d")));
        rowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("End date", contract.EndDate?.ToString("d") ?? "Open-ended"));
        rowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Base salary", contract.BaseSalaryAtSigning.ToString("C"), accent));
        rowGroup.Rows.Add(DocumentBrandingHelper.BuildLabelValueRow("Status", contract.Status.ToString()));

        document.Blocks.Add(detailsTable);

        if (!string.IsNullOrWhiteSpace(contract.Terms))
        {
            document.Blocks.Add(new Paragraph(new Run("Additional terms"))
            {
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 24, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run(contract.Terms)));
        }

        var signatureTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 60, 0, 0) };
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(330) });
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(40) });
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(330) });
        var signatureRowGroup = new TableRowGroup();
        signatureTable.RowGroups.Add(signatureRowGroup);

        var signatureRow = new TableRow();
        signatureRow.Cells.Add(new TableCell(new Paragraph(new Run("Employer signature")))
        {
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 40)
        });
        signatureRow.Cells.Add(new TableCell(new Paragraph()));
        signatureRow.Cells.Add(new TableCell(new Paragraph(new Run("Employee signature")))
        {
            BorderBrush = accent,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 40)
        });
        signatureRowGroup.Rows.Add(signatureRow);

        document.Blocks.Add(signatureTable);

        return document;
    }
}
