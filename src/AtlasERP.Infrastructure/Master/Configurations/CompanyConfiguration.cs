using AtlasERP.Core.Domain.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.Master.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.LegalName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.LogoPath).HasMaxLength(500);
        builder.Property(c => c.BrandColorHex).HasMaxLength(20);
        builder.Property(c => c.ConnectionString).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.InvoiceTemplateRef).HasMaxLength(200);
        builder.Property(c => c.PayslipTemplateRef).HasMaxLength(200);
    }
}
