using AtlasERP.Core.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class PayslipLineItemConfiguration : IEntityTypeConfiguration<PayslipLineItem>
{
    public void Configure(EntityTypeBuilder<PayslipLineItem> builder)
    {
        builder.ToTable("PayslipLineItems");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Description).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");
    }
}
