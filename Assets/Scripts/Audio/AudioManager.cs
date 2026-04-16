using UnityEngine;

/// <summary>
/// Singleton that persists across scenes.
/// Owns two AudioSources: one looping BGM, one for one-shot SFX.
///
/// Volume chain:
///   BGM  volume = MusicVolume  * MasterVolume
///   SFX  volume = SFXVolume    * MasterVolume  (applied per PlayOneShot call)
///
/// Call ApplyVolumes() whenever Master or Music sliders change (VolumeSettings does this).
/// SFX volume is sampled at play-time so no callback is needed for the SFX slider.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] AudioClip backgroundMusic;
    [SerializeField] AudioClip tankAttackSFX;
    [SerializeField] AudioClip fighterAttackSFX;
    [SerializeField] AudioClip rangerAttackSFX;
    [SerializeField] AudioClip enemyShooterSFX;

    AudioSource _bgm;
    AudioSource _sfx;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bgm            = gameObject.AddComponent<AudioSource>();
        _bgm.clip       = backgroundMusic;
        _bgm.loop       = true;
        _bgm.playOnAwake = false;

        _sfx            = gameObject.AddComponent<AudioSource>();
        _sfx.loop       = false;
        _sfx.playOnAwake = false;
    }

    void Start()
    {
        ApplyVolumes();
        if (_bgm.clip != null)
            _bgm.Play();
    }

    // ── Volume ────────────────────────────────────────────────────────────────

    /// <summary>Re-applies BGM volume from current VolumeSettings values.
    /// Called automatically by VolumeSettings when Master or Music changes.</summary>
    public void ApplyVolumes()
    {
        if (VolumeSettings.Instance == null) return;
        _bgm.volume = VolumeSettings.Instance.MusicVolume * VolumeSettings.Instance.MasterVolume;
    }

    // ── SFX ───────────────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        float vol = VolumeSettings.Instance != null
            ? VolumeSettings.Instance.SFXVolume * VolumeSettings.Instance.MasterVolume
            : 1f;
        _sfx.PlayOneShot(clip, vol);
    }

    /// <summary>Plays the correct attack SFX for the given player class.</summary>
    public void PlayPlayerAttack(ClassType classType)
    {
        switch (classType)
        {
            case ClassType.Tank:    PlaySFX(tankAttackSFX);    break;
            case ClassType.Fighter: PlaySFX(fighterAttackSFX); break;
            case ClassType.Ranger:  PlaySFX(rangerAttackSFX);  break;
        }
    }

    public void PlayEnemyShooter() => PlaySFX(enemyShooterSFX);
}
