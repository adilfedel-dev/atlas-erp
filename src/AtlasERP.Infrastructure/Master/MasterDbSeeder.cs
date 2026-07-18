using AtlasERP.Core.Domain.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtlasERP.Infrastructure.Master;

/// <summary>
/// First-run seed data so the app is actually usable right after `dotnet ef database update`:
/// one Administrator role/permission, one admin login, and four placeholder companies (one
/// per brand) so company switching can be exercised end to end. Replace the placeholder
/// connection strings and admin password before anything resembling production use —
/// see SETUP.md.
/// </summary>
public class MasterDbSeeder
{
    private readonly ILogger<MasterDbSeeder> _logger;

    public MasterDbSeeder(ILogger<MasterDbSeeder> logger)
    {
        _logger = logger;
    }

    public async Task SeedAsync(MasterDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        _logger.LogInformation("Master DB is empty — seeding default admin user, role, and placeholder companies.");

        var adminPermission = new Permission
        {
            Code = "system.full_access",
            Name = "Full system access",
            Description = "Unrestricted access to all modules and companies."
        };

        var adminRole = new Role
        {
            Name = "Administrator",
            Description = "Built-in role with unrestricted access."
        };
        adminRole.RolePermissions.Add(new RolePermission { Role = adminRole, Permission = adminPermission });

        var (hash, salt) = PasswordHasher.HashPassword("Admin123!");
        var adminUser = new ApplicationUser
        {
            Username = "admin",
            DisplayName = "Administrator",
            Email = "admin@atlaserp.local",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };
        adminUser.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });

        var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);

        var companies = new[]
        {
            new Company
            {
                Code = "BRAND1",
                Name = "Brand One",
                LegalName = "Brand One Ltd.",
                ConnectionString = $"Data Source={Path.Combine(dataDirectory, "AtlasERP_Brand1.db")}"
            },
            new Company
            {
                Code = "BRAND2",
                Name = "Brand Two",
                LegalName = "Brand Two Ltd.",
                ConnectionString = $"Data Source={Path.Combine(dataDirectory, "AtlasERP_Brand2.db")}"
            },
            new Company
            {
                Code = "BRAND3",
                Name = "Brand Three",
                LegalName = "Brand Three Ltd.",
                ConnectionString = $"Data Source={Path.Combine(dataDirectory, "AtlasERP_Brand3.db")}"
            },
            new Company
            {
                Code = "BRAND4",
                Name = "Brand Four",
                LegalName = "Brand Four Ltd.",
                ConnectionString = $"Data Source={Path.Combine(dataDirectory, "AtlasERP_Brand4.db")}"
            }
        };

        foreach (var company in companies)
        {
            adminUser.CompanyAccessEntries.Add(new UserCompanyAccess { User = adminUser, Company = company });
        }

        db.Permissions.Add(adminPermission);
        db.Roles.Add(adminRole);
        db.Users.Add(adminUser);
        db.Companies.AddRange(companies);

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seed complete. Login with username 'admin' / password 'Admin123!' — change this immediately.");
    }
}
