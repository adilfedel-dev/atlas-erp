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

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 13
        };

        document.Blocks.Add(new Paragraph(new Run("EMPLOYMENT CONTRACT"))
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2)
        });

        document.Blocks.Add(new Paragraph(new Run(company.LegalName))
        {
            FontSize = 13,
            Foreground = Brushes.Gray,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 28)
        });

        var detailsTable = new Table { CellSpacing = 0 };
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(170) });
        detailsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var rowGroup = new TableRowGroup();
        detailsTable.RowGroups.Add(rowGroup);

        void AddRow(string label, string value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.SemiBold })
            {
                Padding = new Thickness(0, 5, 0, 5)
            });
            row.Cells.Add(new TableCell(new Paragraph(new Run(value)))
            {
                Padding = new Thickness(0, 5, 0, 5)
            });
            rowGroup.Rows.Add(row);
        }

        AddRow("Employee", $"{employee.FirstName} {employee.LastName} ({employee.EmployeeCode})");
        AddRow("Job title", employee.JobTitle);
        AddRow("Contract type", contract.ContractType.ToString());
        AddRow("Start date", contract.StartDate.ToString("d"));
        AddRow("End date", contract.EndDate?.ToString("d") ?? "Open-ended");
        AddRow("Base salary", contract.BaseSalaryAtSigning.ToString("C"));
        AddRow("Status", contract.Status.ToString());

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
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(40) });
        signatureTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var signatureRowGroup = new TableRowGroup();
        signatureTable.RowGroups.Add(signatureRowGroup);

        var signatureRow = new TableRow();
        signatureRow.Cells.Add(new TableCell(new Paragraph(new Run("Employer signature")))
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 40)
        });
        signatureRow.Cells.Add(new TableCell(new Paragraph()));
        signatureRow.Cells.Add(new TableCell(new Paragraph(new Run("Employee signature")))
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 40)
        });
        signatureRowGroup.Rows.Add(signatureRow);

        document.Blocks.Add(signatureTable);

        return document;
    }
}
