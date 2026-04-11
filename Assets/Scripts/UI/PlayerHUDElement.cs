using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One player's section of the HUD: HP bar, armor overlay, XP bar, level, class icon, respawn.
/// Assign all references from the scene.
/// </summary>
public class PlayerHUDElement : MonoBehaviour
{
    [Header("Health")]
    public Slider   hpBar;
    public Slider   armorBar;       // secondary bar shown over HP

    [Header("XP / Level")]
    public Slider   xpBar;
    public TextMeshProUGUI levelText;

    [Header("Class")]
    public Image    classIcon;
    public TextMeshProUGUI className;

    [Header("Respawn")]
    public GameObject     respawnPanel;
    public TextMeshProUGUI respawnTimerText;

    private PlayerStats   _stats;
    private PlayerLeveling _leveling;

    public void Bind(PlayerStats stats, PlayerLeveling leveling)
    {
        _stats    = stats;
        _leveling = leveling;

        stats.OnHealthChanged    += UpdateHP;
        leveling.OnLevelUp       += UpdateLevel;

        // Initial values
        UpdateHP(stats.CurrentHealth, stats.maxHealth);
        UpdateLevel(leveling);
    }

    private void OnDestroy()
    {
        if (_stats    != null) _stats.OnHealthChanged  -= UpdateHP;
        if (_leveling != null) _leveling.OnLevelUp      -= UpdateLevel;
    }

    private void Update()
    {
        if (_stats == null || _leveling == null) return;
        // Armor bar follows actual armor value (normalized to some max)
        if (armorBar != null) armorBar.value = Mathf.Clamp01(_stats.armor / 100f);
        // XP bar
        if (xpBar    != null) xpBar.value    = Mathf.Clamp01(_leveling.XP / _leveling.XPToNext);
    }

    private void UpdateHP(float current, float max)
    {
        if (hpBar != null) hpBar.value = current / max;
    }

    private void UpdateLevel(PlayerLeveling lv)
    {
        if (levelText != null) levelText.SetText($"Lv.{lv.Level}");
        if (xpBar     != null) xpBar.value = Mathf.Clamp01(lv.XP / lv.XPToNext);
    }

    public void ShowDead(bool dead)
    {
        if (respawnPanel != null) respawnPanel.SetActive(dead);
    }

    public void SetRespawnTimer(float seconds)
    {
        if (respawnTimerText != null)
            respawnTimerText.SetText($"{Mathf.CeilToInt(seconds)}s");
    }

    /// <summary>Call after class is selected to update icon / name.</summary>
    public void SetClass(ClassDefinitionSO def)
    {
        if (classIcon != null && def.classIcon != null)
            classIcon.sprite = def.classIcon;
        if (className  != null)
            className.SetText(def.className);
    }
}
