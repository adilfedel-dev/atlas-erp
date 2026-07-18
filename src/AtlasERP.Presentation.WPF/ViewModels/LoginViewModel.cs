using AtlasERP.Core.Application.Abstractions;
using AtlasERP.Core.Domain.Master;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtlasERP.Presentation.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public event EventHandler<ApplicationUser>? LoginSucceeded;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Enter a username and password.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var user = await _authenticationService.AuthenticateAsync(Username, Password);
            if (user is null)
            {
                ErrorMessage = "Invalid username or password.";
                return;
            }

            LoginSucceeded?.Invoke(this, user);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not sign in: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
