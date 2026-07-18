using AtlasERP.Core.Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class TravelExpenseReportConfiguration : IEntityTypeConfiguration<TravelExpenseReport>
{
    public void Configure(EntityTypeBuilder<TravelExpenseReport> builder)
    {
        builder.ToTable("TravelExpenseReports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Destination).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Purpose).IsRequired().HasMaxLength(500);
        builder.Property(r => r.OwnerApproverName).HasMaxLength(200);
        builder.Property(r => r.OwnerSignaturePath).HasMaxLength(500);
        builder.Property(r => r.AccountantApproverName).HasMaxLength(200);
        builder.Property(r => r.AccountantSignaturePath).HasMaxLength(500);

        builder.Ignore(r => r.Total);
        builder.Ignore(r => r.IsFullyApproved);

        builder.HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.LineItems)
            .WithOne(l => l.Report)
            .HasForeignKey(l => l.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
