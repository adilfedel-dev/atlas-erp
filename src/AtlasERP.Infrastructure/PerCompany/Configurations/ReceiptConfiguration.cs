using AtlasERP.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Amount).HasColumnType("decimal(18,2)");
        builder.Property(r => r.Notes).HasMaxLength(1000);
    }
}
