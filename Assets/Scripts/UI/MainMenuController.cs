using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the start-menu buttons. Wired via persistent listeners by MenuSetup.cs.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] SettingsPanelController settingsPanel;

    public void OnClickStart()   => SceneManager.LoadScene("GameScene");
    public void OnClickOptions() => settingsPanel.Open();
    public void OnClickExit()    => Application.Quit();
}
