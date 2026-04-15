/// <summary>
/// Sits on the settings panel root. Lets external buttons call Open/Close
/// without needing a direct GameObject.SetActive reference.
/// </summary>
public class SettingsPanelController : UnityEngine.MonoBehaviour
{
    public void Open()  => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);
}
