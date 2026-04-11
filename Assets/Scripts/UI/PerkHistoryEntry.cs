using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkHistoryEntry : MonoBehaviour
{
    public Image           icon;
    public TextMeshProUGUI perkName;
    public TextMeshProUGUI description;

    public void Setup(PerkSO perk)
    {
        if (icon        != null && perk.icon != null) icon.sprite = perk.icon;
        if (perkName    != null) perkName.SetText(perk.perkName);
        if (description != null) description.SetText(perk.description);
    }
}
