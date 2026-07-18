using AtlasERP.Core.Domain.Common;

namespace AtlasERP.Core.Domain.HumanResources;

public class Department : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
