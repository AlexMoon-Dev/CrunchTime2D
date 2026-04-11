using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single selectable perk card in the level-up UI.
/// </summary>
public class PerkCardUI : MonoBehaviour
{
    [Header("Display")]
    public Image           perkIcon;
    public TextMeshProUGUI perkName;
    public TextMeshProUGUI perkDescription;
    public Button          selectButton;
    public GameObject      lockedOverlay;  // shown when class is locked (class selection only)

    private PerkSO _perk;
    private Action<PerkSO> _onSelected;

    public void Setup(PerkSO perk, Action<PerkSO> onSelected, bool locked = false)
    {
        _perk       = perk;
        _onSelected = onSelected;

        if (perkIcon        != null && perk.icon != null) perkIcon.sprite = perk.icon;
        if (perkName        != null) perkName.SetText(perk.perkName);
        if (perkDescription != null) perkDescription.SetText(perk.description);
        if (lockedOverlay   != null) lockedOverlay.SetActive(locked);

        selectButton?.onClick.RemoveAllListeners();
        if (!locked)
            selectButton?.onClick.AddListener(() => _onSelected?.Invoke(_perk));

        if (selectButton != null) selectButton.interactable = !locked;
    }

    public void SetLocked(bool locked)
    {
        if (lockedOverlay != null) lockedOverlay.SetActive(locked);
        if (selectButton  != null) selectButton.interactable = !locked;
    }
}
