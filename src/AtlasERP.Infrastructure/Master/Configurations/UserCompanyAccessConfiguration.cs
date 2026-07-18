using AtlasERP.Core.Domain.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.Master.Configurations;

public class UserCompanyAccessConfiguration : IEntityTypeConfiguration<UserCompanyAccess>
{
    public void Configure(EntityTypeBuilder<UserCompanyAccess> builder)
    {
        builder.ToTable("UserCompanyAccess");
        builder.HasKey(a => new { a.UserId, a.CompanyId });

        builder.HasOne(a => a.User)
            .WithMany(u => u.CompanyAccessEntries)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Company)
            .WithMany(c => c.UserAccessEntries)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
