using AtlasERP.Core.Domain.Common;
using AtlasERP.Core.Domain.HumanResources.Enums;

namespace AtlasERP.Core.Domain.HumanResources;

/// <summary>
/// Lives in a per-company database — an employee belongs to exactly one brand/company,
/// never shared across the ones in the Master registry.
/// </summary>
public class Employee : EntityBase
{
    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; } = Gender.Unspecified;
    public DateTime? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }

    public string? PersonalEmail { get; set; }
    public string? WorkEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? PhotoPath { get; set; }

    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

    public string JobTitle { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public decimal BaseSalary { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }

    public string? Notes { get; set; }
}
