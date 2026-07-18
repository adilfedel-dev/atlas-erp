using AtlasERP.Core.Domain.Common;
using AtlasERP.Core.Domain.Expenses.Enums;
using AtlasERP.Core.Domain.HumanResources;

namespace AtlasERP.Core.Domain.Expenses;

public class TravelExpenseReport : EntityBase
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public TravelExpenseStatus Status { get; set; } = TravelExpenseStatus.Draft;

    public string? OwnerApproverName { get; set; }
    public string? OwnerSignaturePath { get; set; }
    public DateTime? OwnerApprovedAtUtc { get; set; }

    public string? AccountantApproverName { get; set; }
    public string? AccountantSignaturePath { get; set; }
    public DateTime? AccountantApprovedAtUtc { get; set; }

    public ICollection<TravelExpenseLineItem> LineItems { get; set; } = new List<TravelExpenseLineItem>();

    public decimal Total => LineItems.Sum(l => l.Amount);

    public bool IsFullyApproved => OwnerApprovedAtUtc is not null && AccountantApprovedAtUtc is not null;
}
