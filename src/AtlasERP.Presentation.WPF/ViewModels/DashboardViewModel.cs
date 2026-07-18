using System.Collections.ObjectModel;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.HumanResources.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

/// <summary>A single bar in a hand-rolled chart — BarHeight is pre-computed in pixels so the view needs no converters.</summary>
public record ChartPoint(string Label, decimal Value, double BarHeight);

/// <summary>
/// Landing page with a few real stats and month-by-month trend charts pulled from the
/// current company's data. Will grow with more tiles as later modules land.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private const double MaxBarHeightPixels = 130;
    private const int TrendMonths = 6;

    private readonly IEmployeeService _employeeService;
    private readonly IPayrollService _payrollService;
    private readonly IInvoiceService _invoiceService;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private string _companyDisplayName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogo))]
    private string? _logoPath;

    public bool HasLogo => !string.IsNullOrWhiteSpace(LogoPath);

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _totalEmployeeCount;

    [ObservableProperty]
    private int _activeEmployeeCount;

    [ObservableProperty]
    private int _onLeaveEmployeeCount;

    [ObservableProperty]
    private ObservableCollection<ChartPoint> _headcountTrend = new();

    [ObservableProperty]
    private ObservableCollection<ChartPoint> _payrollCostTrend = new();

    [ObservableProperty]
    private ObservableCollection<ChartPoint> _invoicedTrend = new();

    public DashboardViewModel(
        ICompanyContextService companyContextService,
        IEmployeeService employeeService,
        IPayrollService payrollService,
        IInvoiceService invoiceService)
    {
        _employeeService = employeeService;
        _payrollService = payrollService;
        _invoiceService = invoiceService;

        var company = companyContextService.CurrentCompany;
        CompanyDisplayName = company?.Name ?? "your company";
        LogoPath = company?.LogoPath;
        WelcomeMessage = $"Welcome back — here's what's happening at {CompanyDisplayName}";

        _ = LoadStatsAsync();
    }

    [RelayCommand]
    private async Task LoadStatsAsync()
    {
        IsBusy = true;
        try
        {
            var months = Enumerable.Range(0, TrendMonths)
                .Select(offset => DateTime.Today.AddMonths(-(TrendMonths - 1) + offset))
                .Select(d => new DateTime(d.Year, d.Month, 1))
                .ToList();

            var employees = await _employeeService.GetAllAsync();
            TotalEmployeeCount = employees.Count;
            ActiveEmployeeCount = employees.Count(e => e.EmploymentStatus == EmploymentStatus.Active);
            OnLeaveEmployeeCount = employees.Count(e => e.EmploymentStatus == EmploymentStatus.OnLeave);

            var headcountByMonth = months.Select(monthStart =>
            {
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var count = employees.Count(e => e.HireDate <= monthEnd && (e.TerminationDate is null || e.TerminationDate > monthEnd));
                return (monthStart.ToString("MMM"), (decimal)count);
            }).ToList();
            HeadcountTrend = BuildChartPoints(headcountByMonth);

            var runs = await _payrollService.GetAllRunsAsync();
            var payrollByMonth = months.Select(monthStart =>
            {
                var run = runs.FirstOrDefault(r => r.PeriodYear == monthStart.Year && r.PeriodMonth == monthStart.Month);
                var total = run?.Payslips.Sum(p => p.NetPay) ?? 0m;
                return (monthStart.ToString("MMM"), total);
            }).ToList();
            PayrollCostTrend = BuildChartPoints(payrollByMonth);

            var invoices = await _invoiceService.GetAllAsync();
            var invoicedByMonth = months.Select(monthStart =>
            {
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var total = invoices
                    .Where(i => i.IssueDate >= monthStart && i.IssueDate <= monthEnd)
                    .Sum(i => i.Total);
                return (monthStart.ToString("MMM"), total);
            }).ToList();
            InvoicedTrend = BuildChartPoints(invoicedByMonth);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static ObservableCollection<ChartPoint> BuildChartPoints(IReadOnlyList<(string Label, decimal Value)> points)
    {
        var maxValue = points.Count == 0 ? 0m : points.Max(p => p.Value);
        return new ObservableCollection<ChartPoint>(points.Select(p =>
        {
            var height = maxValue > 0 ? (double)(p.Value / maxValue) * MaxBarHeightPixels : 0;
            return new ChartPoint(p.Label, p.Value, Math.Max(height, 3));
        }));
    }
}
