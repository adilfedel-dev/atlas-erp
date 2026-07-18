namespace AtlasERP.Core.Application.Abstractions;

public interface ICompanySettingsService
{
    Task UpdateBrandingAsync(Guid companyId, string name, string legalName, string? logoPath, string? brandColorHex, CancellationToken cancellationToken = default);
}
