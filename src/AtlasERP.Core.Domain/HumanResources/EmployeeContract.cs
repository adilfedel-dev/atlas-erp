using AtlasERP.Core.Domain.Common;
using AtlasERP.Core.Domain.HumanResources.Enums;

namespace AtlasERP.Core.Domain.HumanResources;

public class EmployeeContract : EntityBase
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public ContractType ContractType { get; set; } = ContractType.Permanent;
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? SignedDate { get; set; }

    /// <summary>Salary term captured at signing — kept independent of Employee.BaseSalary, which may change later.</summary>
    public decimal BaseSalaryAtSigning { get; set; }

    public string? Terms { get; set; }
    public string? DocumentPath { get; set; }
}
