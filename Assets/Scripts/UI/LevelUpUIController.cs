using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Full-screen level-up overlay. P1 choices on left, P2 on right.
/// Also handles class selection (same layout, just ClassDefinitionSO as "perks").
/// </summary>
public class LevelUpUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panel;
    public Transform  p1CardContainer;
    public Transform  p2CardContainer;
    public TextMeshProUGUI p1StatusText;
    public TextMeshProUGUI p2StatusText;

    [Header("1P mode — parent that groups all P2 perk UI")]
    public GameObject p2PerkColumn;

    [Header("Prefab")]
    public PerkCardUI perkCardPrefab;

    private bool _p1Confirmed = false;
    private bool _p2Confirmed = false;

    // Class selection mode — cards are ClassDefinitionSOs dressed as PerkSOs
    private bool _isClassSelection = false;

    private void OnEnable()  => GameManager.OnGameStateChanged += OnStateChanged;
    private void OnDisable() => GameManager.OnGameStateChanged -= OnStateChanged;

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Wave) Hide();
    }

    public void Show(List<PerkSO> p1Offerings, List<PerkSO> p2Offerings)
    {
        panel?.SetActive(true);
        _p1Confirmed = false;
        _p2Confirmed = false;

        bool singlePlayer = GameSetupManager.Instance != null
            && GameSetupManager.Instance.PlayerCount == 1;

        PopulateCards(p1CardContainer, p1Offerings, 0);
        p1StatusText?.SetText("Choose a perk");

        // Show or hide the P2 perk column based on player count
        if (p2PerkColumn != null)
        {
            p2PerkColumn.SetActive(!singlePlayer);
        }
        else
        {
            if (p2CardContainer != null) p2CardContainer.gameObject.SetActive(!singlePlayer);
            if (p2StatusText    != null) p2StatusText.gameObject.SetActive(!singlePlayer);
        }

        if (!singlePlayer)
        {
            PopulateCards(p2CardContainer, p2Offerings, 1);
            p2StatusText?.SetText("Choose a perk");
        }
    }

    public void Hide() => panel?.SetActive(false);

    public void SetWaiting(int playerIndex)
    {
        if (playerIndex == 0) { _p1Confirmed = true; p1StatusText?.SetText("Waiting..."); }
        else                  { _p2Confirmed = true; p2StatusText?.SetText("Waiting..."); }
    }

    private void PopulateCards(Transform container, List<PerkSO> perks, int playerIndex)
    {
        // Clear existing
        foreach (Transform child in container)
            Destroy(child.gameObject);

        foreach (var perk in perks)
        {
            var card = Instantiate(perkCardPrefab, container);
            card.Setup(perk, (chosen) => OnPerkChosen(playerIndex, chosen));
        }

        // Pad with empty if fewer than 3
        // TODO: show "No more perks available" placeholder
    }

    private void OnPerkChosen(int playerIndex, PerkSO perk)
    {
        // Disable all cards for this player (can't change mind)
        var container = playerIndex == 0 ? p1CardContainer : p2CardContainer;
        foreach (Transform child in container)
            child.GetComponent<PerkCardUI>()?.SetLocked(true);

        LevelUpManager.Instance?.ConfirmPerk(playerIndex, perk);
    }

    // ── Class Selection (reuse same UI, perks wrapped from class SO) ──────────

    [Header("Class Selection")]
    public ClassSelectionUI classSelectionUI;

    public void ShowClassSelection()
    {
        _isClassSelection = true;
        panel?.SetActive(true);
        classSelectionUI?.gameObject.SetActive(true);
    }
}
