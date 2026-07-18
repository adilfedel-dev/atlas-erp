using AtlasERP.Core.Domain.HumanResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class EmployeeContractConfiguration : IEntityTypeConfiguration<EmployeeContract>
{
    public void Configure(EntityTypeBuilder<EmployeeContract> builder)
    {
        builder.ToTable("EmployeeContracts");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.BaseSalaryAtSigning).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Terms).HasMaxLength(2000);
        builder.Property(c => c.DocumentPath).HasMaxLength(500);

        builder.HasOne(c => c.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
