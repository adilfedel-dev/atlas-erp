using System.IO;
using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Presentation.WPF.Printing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ICompanyContextService _companyContextService;
    private readonly ICompanySettingsService _companySettingsService;
    private Guid _companyId;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _legalName = string.Empty;
    [ObservableProperty] private string? _logoPath;
    [ObservableProperty] private string? _brandColorHex;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private bool _isBusy;

    public SettingsViewModel(ICompanyContextService companyContextService, ICompanySettingsService companySettingsService)
    {
        _companyContextService = companyContextService;
        _companySettingsService = companySettingsService;
        LoadFromCurrentCompany();
    }

    public void LoadFromCurrentCompany()
    {
        var company = _companyContextService.CurrentCompany;
        if (company is null)
        {
            return;
        }

        _companyId = company.Id;
        Name = company.Name;
        LegalName = company.LegalName;
        LogoPath = company.LogoPath;
        BrandColorHex = company.BrandColorHex;
        ErrorMessage = null;
        StatusMessage = null;
    }

    [RelayCommand]
    private void BrowseLogo()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Title = "Choose a logo"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var logosDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logos");
            Directory.CreateDirectory(logosDirectory);

            var extension = Path.GetExtension(dialog.FileName);
            var destinationPath = Path.Combine(logosDirectory, $"{_companyId}{extension}");
            File.Copy(dialog.FileName, destinationPath, overwrite: true);

            LogoPath = destinationPath;

            var extractedColor = LogoColorExtractor.TryExtractDominantColorHex(destinationPath);
            if (extractedColor is not null)
            {
                BrandColorHex = extractedColor;
            }

            StatusMessage = "Logo updated — click Save to apply.";
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not use that image: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(LegalName))
        {
            ErrorMessage = "Display name and legal name are required.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = null;
        try
        {
            await _companySettingsService.UpdateBrandingAsync(_companyId, Name, LegalName, LogoPath, BrandColorHex);

            // Refresh the in-memory current company so the sidebar and any documents
            // printed afterward pick up the change immediately, without re-logging in.
            var current = _companyContextService.CurrentCompany;
            if (current is not null)
            {
                current.Name = Name;
                current.LegalName = LegalName;
                current.LogoPath = LogoPath;
                current.BrandColorHex = BrandColorHex;
                _companyContextService.SetCurrentCompany(current);
            }

            StatusMessage = "Saved.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not save: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
