using AtlasERP.Core.Domain.HumanResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.Company.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.EmployeeCode).IsUnique();

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.NationalId).HasMaxLength(50);
        builder.Property(e => e.PassportNumber).HasMaxLength(50);

        builder.Property(e => e.PersonalEmail).HasMaxLength(200);
        builder.Property(e => e.WorkEmail).HasMaxLength(200);
        builder.Property(e => e.PhoneNumber).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.PhotoPath).HasMaxLength(500);

        builder.Property(e => e.JobTitle).IsRequired().HasMaxLength(150);

        builder.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)");
        builder.Property(e => e.BankName).HasMaxLength(200);
        builder.Property(e => e.BankAccountNumber).HasMaxLength(100);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
