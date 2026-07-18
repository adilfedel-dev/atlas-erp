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

        var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

        document.PageHeight = pageSize.Height;
        document.PageWidth = pageSize.Width;
        document.PagePadding = new Thickness(50);

        // Setting FlowDocument.PageWidth/PageHeight alone isn't enough — the paginator used
        // for the actual print pass caches its own PageSize, and if that's never set
        // explicitly it measures Table star-columns against an effectively-zero width,
        // which is what caused every value column to wrap one character per line.
        IDocumentPaginatorSource paginatorSource = document;
        var paginator = paginatorSource.DocumentPaginator;
        paginator.PageSize = pageSize;

        printDialog.PrintDocument(paginator, jobName);
        return true;
    }
}
