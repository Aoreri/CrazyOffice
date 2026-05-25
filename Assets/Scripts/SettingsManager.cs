using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SettingsManager : MonoBehaviour
{
    [Header("Fullscreen")]
    public Toggle fullscreenToggle;

    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;
    private List<Resolution> resolutions;

    [Header("Quality")]
    public TMP_Dropdown qualityDropdown;

    [Header("Audio")]
    public Slider volumeSlider;

    [Header("FPS Limit")]
    public TMP_Dropdown fpsDropdown;

    private void Start()
    {
        SetupResolutionDropdown();
        SetupQualityDropdown();
        SetupFPSDropdown();

        LoadSettings();

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        volumeSlider.onValueChanged.AddListener(SetVolume);
        fpsDropdown.onValueChanged.AddListener(SetFPSLimit);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // =========================
    // RESOLUTION
    // =========================

    void SetupResolutionDropdown()
    {
        // Deduplicate by width+height, then sort highest to lowest
        resolutions = Screen.resolutions
            .GroupBy(r => new { r.width, r.height })
            .Select(g => g.Last())
            .OrderByDescending(r => r.width)
            .ThenByDescending(r => r.height)
            .ToList();

        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Count; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    // =========================
    // QUALITY
    // =========================

    void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
    }

    // =========================
    // VOLUME
    // =========================

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    // =========================
    // FPS LIMIT
    // =========================

    void SetupFPSDropdown()
    {
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new List<string>
        {
            "Unlimited",
            "240",
            "144",
            "120",
            "60",
            "30"
        });
    }

    public void SetFPSLimit(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = -1; break; // Unlimited
            case 1: Application.targetFrameRate = 240; break;
            case 2: Application.targetFrameRate = 144; break;
            case 3: Application.targetFrameRate = 120; break;
            case 4: Application.targetFrameRate = 60; break;
            case 5: Application.targetFrameRate = 30; break;
        }

        PlayerPrefs.SetInt("FPS", index);
    }

    // =========================
    // LOAD
    // =========================

    void LoadSettings()
    {
        // Fullscreen
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = fullscreen;
        Screen.fullScreen = fullscreen;

        // Resolution
        int resolutionIndex = PlayerPrefs.GetInt("Resolution", resolutionDropdown.value);
        resolutionDropdown.value = resolutionIndex;
        SetResolution(resolutionIndex);

        // Quality
        int qualityIndex = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        qualityDropdown.value = qualityIndex;
        SetQuality(qualityIndex);

        // Volume
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = volume;
        SetVolume(volume);

        // FPS — default 0 = Unlimited
        int fpsIndex = PlayerPrefs.GetInt("FPS", 0);
        fpsDropdown.value = fpsIndex;
        SetFPSLimit(fpsIndex);
    }
}