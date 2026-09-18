using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Suwaki")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectsVolumeSlider;

    [Header("Vibration")]
    [SerializeField] private Toggle vibrationToggle;

    [Header("Obrażenia krytyczne")]
    [SerializeField] private Toggle criticalDamageToggle;


    private void Start()
    {
        LoadSettings();
    }


    // =========================================================
    // MUZYKA
    // =========================================================

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);

        SetMixerVolume(
            "MusicVolume",
            value
        );

        PlayerPrefs.SetFloat(
            "MusicVolume",
            value
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // EFEKTY
    // =========================================================

    public void SetEffectsVolume(float value)
    {
        value = Mathf.Clamp01(value);

        SetMixerVolume(
            "EffectsVolume",
            value
        );

        PlayerPrefs.SetFloat(
            "EffectsVolume",
            value
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // WIBRACJE
    // =========================================================

    public void SetVibration(bool enabled)
    {
        PlayerPrefs.SetInt(
            "VibrationEnabled",
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // OBRAŻENIA KRYTYCZNE
    // =========================================================

    public void SetCriticalDamageVisibility(bool enabled)
    {
        if (GameSettings.Instance == null)
        {
            return;
        }

        GameSettings.Instance.SetShowCriticalDamage(
            enabled
        );
    }


    // =========================================================
    // WCZYTYWANIE USTAWIEŃ
    // =========================================================

    private void LoadSettings()
    {
        float musicVolume =
            PlayerPrefs.GetFloat(
                "MusicVolume",
                1f
            );

        float effectsVolume =
            PlayerPrefs.GetFloat(
                "EffectsVolume",
                1f
            );

        int vibrationEnabled =
            PlayerPrefs.GetInt(
                "VibrationEnabled",
                1
            );


        // =====================================================
        // SUWAK MUZYKI
        // =====================================================

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(
                musicVolume
            );
        }


        // =====================================================
        // SUWAK EFEKTÓW
        // =====================================================

        if (effectsVolumeSlider != null)
        {
            effectsVolumeSlider.SetValueWithoutNotify(
                effectsVolume
            );
        }


        // =====================================================
        // WIBRACJE
        // =====================================================

        if (vibrationToggle != null)
        {
            vibrationToggle.SetIsOnWithoutNotify(
                vibrationEnabled == 1
            );
        }


        // =====================================================
        // OBRAŻENIA KRYTYCZNE
        // =====================================================

        if (criticalDamageToggle != null)
        {
            bool showCriticalDamage = true;

            if (GameSettings.Instance != null)
            {
                showCriticalDamage =
                    GameSettings.Instance.ShowCriticalDamage;
            }

            criticalDamageToggle.SetIsOnWithoutNotify(
                showCriticalDamage
            );
        }


        // =====================================================
        // AUDIO MIXER
        // =====================================================

        SetMixerVolume(
            "MusicVolume",
            musicVolume
        );

        SetMixerVolume(
            "EffectsVolume",
            effectsVolume
        );
    }


    // =========================================================
    // USTAWIENIE MIXERA
    // =========================================================

    private void SetMixerVolume(
        string parameterName,
        float value)
    {
        if (audioMixer == null)
            return;


        float volume =
            Mathf.Clamp(
                value,
                0.0001f,
                1f
            );


        float volumeDb =
            Mathf.Log10(volume) * 20f;


        audioMixer.SetFloat(
            parameterName,
            volumeDb
        );
    }
}