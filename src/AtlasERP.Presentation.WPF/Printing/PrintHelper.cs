using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AtlasERP.Presentation.WPF.Printing;

/// <summary>
/// Thin wrapper around WPF's built-in FlowDocument + PrintDialog pipeline — no external
/// PDF/reporting library involved, since this app has already hit enough dependency
/// installation friction. Good enough for letter-style documents (contracts, payslips).
/// </summary>
public static class PrintHelper
{
    /// <summary>Shows the system print dialog and, if confirmed, prints the document. Returns false if the user cancelled.</summary>
    public static bool Print(FlowDocument document, string jobName)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
        {
            return false;
        }

        document.PageHeight = printDialog.PrintableAreaHeight;
        document.PageWidth = printDialog.PrintableAreaWidth;
        document.PagePadding = new Thickness(50);
        document.ColumnGap = 0;
        document.ColumnWidth = printDialog.PrintableAreaWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, jobName);
        return true;
    }
}
