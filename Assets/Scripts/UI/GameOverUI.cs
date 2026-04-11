using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI waveReachedText;
    public UnityEngine.UI.Button restartButton;

    private void Awake()
    {
        // Subscribe in Awake so the event fires even when this panel is inactive.
        // OnEnable/OnDisable would unsubscribe when panel.SetActive(false) is called
        // in Start(), silently breaking the game-over trigger.
        GameManager.OnGameStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnStateChanged;
    }

    private void Start()
    {
        panel?.SetActive(false);
        restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartGame());
    }

    private void OnStateChanged(GameState state)
    {
        if (state != GameState.GameOver) return;
        panel?.SetActive(true);
        int wave = WaveManager.Instance != null ? WaveManager.Instance.CurrentWave : 0;
        waveReachedText?.SetText($"Survived to Wave {wave}");
    }
}
