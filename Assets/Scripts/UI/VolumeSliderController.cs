using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a Slider. Set <see cref="volumeType"/> in the Inspector.
/// Reads the saved volume on first activation and writes back whenever the slider moves.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderController : MonoBehaviour
{
    public enum VolumeType { Master, SFX, Music }

    [SerializeField] VolumeType volumeType;

    Slider _slider;

    void Start()
    {
        _slider = GetComponent<Slider>();

        // Initialise from saved value without triggering the callback
        _slider.SetValueWithoutNotify(GetSavedValue());

        _slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        if (VolumeSettings.Instance == null) return;
        switch (volumeType)
        {
            case VolumeType.Master: VolumeSettings.Instance.SetMaster(value); break;
            case VolumeType.SFX:   VolumeSettings.Instance.SetSFX(value);    break;
            case VolumeType.Music: VolumeSettings.Instance.SetMusic(value);   break;
        }
    }

    float GetSavedValue()
    {
        if (VolumeSettings.Instance == null) return 1f;
        return volumeType switch
        {
            VolumeType.Master => VolumeSettings.Instance.MasterVolume,
            VolumeType.SFX   => VolumeSettings.Instance.SFXVolume,
            VolumeType.Music => VolumeSettings.Instance.MusicVolume,
            _                => 1f,
        };
    }
}
