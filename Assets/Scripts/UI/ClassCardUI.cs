using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A single class selection card shown during class selection screen.
/// </summary>
public class ClassCardUI : MonoBehaviour
{
    [Header("Display")]
    public Image           classIcon;
    public TextMeshProUGUI classNameText;
    public TextMeshProUGUI descriptionText;
    public Button          selectButton;
    public GameObject      lockedOverlay;

    public ClassType ClassType { get; private set; }

    public void Setup(ClassDefinitionSO def, Action<ClassDefinitionSO> onSelected)
    {
        ClassType = def.classType;
        if (classIcon      != null && def.classIcon != null) classIcon.sprite = def.classIcon;
        if (classNameText  != null) classNameText.SetText(def.className);
        if (descriptionText != null) descriptionText.SetText(def.description);

        selectButton?.onClick.RemoveAllListeners();
        selectButton?.onClick.AddListener(() => onSelected?.Invoke(def));
    }

    public void SetLocked(bool locked)
    {
        if (lockedOverlay != null) lockedOverlay.SetActive(locked);
        if (selectButton  != null) selectButton.interactable = !locked;
    }
}
