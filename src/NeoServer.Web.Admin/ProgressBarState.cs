namespace NeoServer.Web.Admin;

public class ProgressBarState
{
    public bool Visible { get; private set; }
    public event Action? OnChange;

    public void Show()
    {
        if (Visible) return;
        Visible = true;
        OnChange?.Invoke();
    }

    public void Hide()
    {
        if (Visible == false) return;
        Visible = false;
        OnChange?.Invoke();
    }
}