using AtlasERP.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Description).IsRequired().HasMaxLength(300);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");

        builder.Ignore(l => l.LineTotal);
    }
}
