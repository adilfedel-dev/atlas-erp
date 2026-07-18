# AtlasERP — Module 1 (foundation) setup

This is the platform skeleton: Master DB (companies/users/roles), per-company DB with
dynamic connection switching, WPF shell (login → company selector → main window),
Light/Dark theming, and the first real module (Employees — list, add, edit, delete).

WPF only builds and runs on **Windows**. These steps assume you've gotten the `src/`
folder and `AtlasERP.sln` onto a Windows machine (however that happens — we haven't
wired up a GitHub remote yet, so for now that's a manual copy; ask if you want to set
one up).

## Prerequisites

- **.NET 8 SDK** — https://dotnet.microsoft.com/download/dotnet/8.0
- **Visual Studio 2022** with the ".NET desktop development" workload (recommended), or
  just the `dotnet` CLI if you prefer the terminal
- **SQL Server LocalDB** — included with Visual Studio, or install "SQL Server Express
  LocalDB" standalone
- **EF Core CLI tools**, if not already installed:
  ```
  dotnet tool install --global dotnet-ef
  ```

## First-time setup

Run these from the repo root (the folder containing `AtlasERP.sln`).

**1. Restore packages**
```
dotnet restore
```

**2. Generate the Master DB migration**
```
dotnet ef migrations add InitialCreate --project src/AtlasERP.Infrastructure --startup-project src/AtlasERP.Presentation.WPF --context AtlasERP.Infrastructure.Master.MasterDbContext --output-dir Master/Migrations
```

**3. Generate the per-company DB migration**
```
dotnet ef migrations add InitialCreate --project src/AtlasERP.Infrastructure --startup-project src/AtlasERP.Presentation.WPF --context AtlasERP.Infrastructure.Company.CompanyDbContext --output-dir Company/Migrations
```

**4. Run the app**
```
dotnet run --project src/AtlasERP.Presentation.WPF
```

On first launch the app will, in order:
- Apply the Master DB migration (creates `AtlasERP_Master` on LocalDB automatically)
- Seed a default admin user and **four placeholder companies** (Brand One–Four, all
  pointing at LocalDB databases named `AtlasERP_Brand1`–`4`)
- Apply the per-company migration to all four of those databases

**5. Sign in**
- Username: `admin`
- Password: `Admin123!`

There's no password-change screen yet (that's part of a future User Management
module) — this seed is for local development only. Don't point the seeded connection
strings at anything real without changing this first.

## What's here

- `AtlasERP.Core.Domain` — entities only, no dependencies
- `AtlasERP.Core.Application` — service interfaces (`Abstractions/`), the one
  dependency-free implementation (`CompanyContextService`)
- `AtlasERP.Infrastructure` — EF Core DbContexts (`Master/`, `Company/`), service
  implementations, migrations (once generated)
- `AtlasERP.Presentation.WPF` — views, viewmodels (CommunityToolkit.Mvvm), DI/host
  wiring in `App.xaml.cs`, theming in `Themes/`

## Adding the next module

Same pattern as Employees: a domain entity under `Core.Domain`, an
`IEntityTypeConfiguration` + DbSet in `CompanyDbContext`, a service interface in
`Core.Application/Abstractions` + implementation in `Infrastructure`, a
ViewModel/View pair in `Presentation.WPF`, then a `DataTemplate` + nav button in
`MainWindow.xaml`. After adding entities, generate a new migration:
```
dotnet ef migrations add <Name> --project src/AtlasERP.Infrastructure --startup-project src/AtlasERP.Presentation.WPF --context AtlasERP.Infrastructure.Company.CompanyDbContext --output-dir Company/Migrations
```
It gets applied to all four company databases automatically on next launch.
