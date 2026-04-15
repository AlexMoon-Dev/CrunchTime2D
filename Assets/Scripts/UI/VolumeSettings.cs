using UnityEngine;
// To wire an AudioMixer tomorrow:
//   1. Add:  public AudioMixer audioMixer;
//   2. Expose three parameters in the mixer named "MasterVolume", "SFXVolume", "MusicVolume"
//   3. Uncomment the audioMixer?.SetFloat lines in Apply()

/// <summary>
/// Singleton that survives scene loads. Stores Master / SFX / Music volumes
/// in PlayerPrefs so they persist between sessions.
/// </summary>
public class VolumeSettings : MonoBehaviour
{
    public static VolumeSettings Instance { get; private set; }

    const string KEY_MASTER = "vol_master";
    const string KEY_SFX    = "vol_sfx";
    const string KEY_MUSIC  = "vol_music";

    public float MasterVolume { get; private set; }
    public float SFXVolume    { get; private set; }
    public float MusicVolume  { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        SFXVolume    = PlayerPrefs.GetFloat(KEY_SFX,    1f);
        MusicVolume  = PlayerPrefs.GetFloat(KEY_MUSIC,  1f);
    }

    public void SetMaster(float v) { MasterVolume = v; Save(KEY_MASTER, "MasterVolume", v); }
    public void SetSFX(float v)    { SFXVolume    = v; Save(KEY_SFX,    "SFXVolume",    v); }
    public void SetMusic(float v)  { MusicVolume  = v; Save(KEY_MUSIC,  "MusicVolume",  v); }

    void Save(string prefKey, string mixerParam, float linear)
    {
        PlayerPrefs.SetFloat(prefKey, linear);
        // float dB = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        // audioMixer?.SetFloat(mixerParam, dB);
    }
}
