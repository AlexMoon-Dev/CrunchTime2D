using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Displayed at game start. Both players pick a class.
/// When P1 confirms, their class is locked and P2's matching card grays out.
/// </summary>
public class ClassSelectionUI : MonoBehaviour
{
    [Header("Class Cards per Player")]
    public ClassCardUI[] p1Cards;   // 3 cards: Tank, Fighter, Ranger
    public ClassCardUI[] p2Cards;

    [Header("Status")]
    public TextMeshProUGUI p1Status;
    public TextMeshProUGUI p2Status;

    [Header("1P mode — assign the parent that groups all P2 UI")]
    public GameObject p2Column;   // hides entire P2 side when playing solo

    [Header("Class Definitions (assign in inspector)")]
    public ClassDefinitionSO tankDef;
    public ClassDefinitionSO fighterDef;
    public ClassDefinitionSO rangerDef;

    private void Start()
    {
        var defs = new[] { tankDef, fighterDef, rangerDef };

        for (int i = 0; i < 3 && i < defs.Length; i++)
        {
            if (defs[i] == null) continue;
            if (i < p1Cards.Length) p1Cards[i]?.Setup(defs[i], (d) => OnPlayerChoose(0, d));
            if (i < p2Cards.Length) p2Cards[i]?.Setup(defs[i], (d) => OnPlayerChoose(1, d));
        }

        ClassManager.OnClassLocked += OnClassLocked;
        GameManager.OnGameStateChanged += OnStateChanged;

        // LevelUpPanel starts inactive, so Start() fires after ShowClassSelection()
        // activates it — meaning SetState(ClassSelection) already fired and was missed.
        // Apply layout immediately if we're already in that state.
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.ClassSelection)
            OnStateChanged(GameState.ClassSelection);
    }

    private void OnDestroy()
    {
        ClassManager.OnClassLocked -= OnClassLocked;
        GameManager.OnGameStateChanged -= OnStateChanged;
    }

    [Header("1P scale — how much bigger P1 cards get in single-player")]
    public float singlePlayerScale = 1.5f;

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.ClassSelection)
        {
            // Apply() has been called by now — PlayerCount is correct
            bool singlePlayer = GameSetupManager.Instance != null
                && GameSetupManager.Instance.PlayerCount == 1;

            if (p2Column != null)
                p2Column.SetActive(!singlePlayer);
            else
            {
                foreach (var c in p2Cards) c?.gameObject.SetActive(!singlePlayer);
                p2Status?.gameObject.SetActive(!singlePlayer);
            }

            // Scale up the P1 content when P2 is hidden so it fills the space
            float s = singlePlayer ? singlePlayerScale : 1f;
            transform.localScale = new Vector3(s, s, 1f);
        }
        else if (state == GameState.Wave)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnPlayerChoose(int playerIndex, ClassDefinitionSO def)
    {
        bool ok = ClassManager.Instance?.TryConfirmClass(playerIndex, def) ?? false;
        if (!ok) return;

        if (playerIndex == 0) p1Status?.SetText($"Chose {def.className}. Waiting...");
        else                  p2Status?.SetText($"Chose {def.className}. Waiting...");
    }

    private void OnClassLocked(ClassType ct)
    {
        // Gray out matching cards for both players
        void GrayOut(ClassCardUI[] cards)
        {
            foreach (var c in cards)
                if (c.ClassType == ct) c.SetLocked(true);
        }
        GrayOut(p1Cards);
        GrayOut(p2Cards);
    }
}
