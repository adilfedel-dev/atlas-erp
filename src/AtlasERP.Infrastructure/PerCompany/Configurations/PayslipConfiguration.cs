using AtlasERP.Core.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.GrossPay).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalBonuses).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalDeductions).HasColumnType("decimal(18,2)");
        builder.Property(p => p.NetPay).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.LineItems)
            .WithOne(l => l.Payslip)
            .HasForeignKey(l => l.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
