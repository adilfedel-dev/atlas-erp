using AtlasERP.Core.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.Property(i => i.TaxRatePercent).HasColumnType("decimal(5,2)");
        builder.Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Total).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Notes).HasMaxLength(2000);

        builder.HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.LineItems)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Receipts)
            .WithOne(r => r.Invoice)
            .HasForeignKey(r => r.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
