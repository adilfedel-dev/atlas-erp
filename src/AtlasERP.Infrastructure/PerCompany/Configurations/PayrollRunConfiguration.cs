using AtlasERP.Core.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.PeriodYear, r.PeriodMonth }).IsUnique();

        builder.HasMany(r => r.Payslips)
            .WithOne(p => p.PayrollRun)
            .HasForeignKey(p => p.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
