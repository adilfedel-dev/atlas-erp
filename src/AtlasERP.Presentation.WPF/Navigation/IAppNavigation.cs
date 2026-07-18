namespace AtlasERP.Presentation.WPF.Navigation;

/// <summary>
/// Coordinates the top-level window flow: login → (company selector, if more than one
/// company is accessible) → main shell. Kept separate from the Views themselves so no
/// view needs to know what comes after it — it just raises an event and this decides.
/// </summary>
public interface IAppNavigation
{
    void ShowLogin();
    void ShowMainWindow();
}
