using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Fullscreen")]
    public Toggle fullscreenToggle;

    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

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


    void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;

        System.Collections.Generic.List<string> options =
            new System.Collections.Generic.List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option =
                resolutions[i].width + " x " +
                resolutions[i].height;

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

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen
        );

        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    // =========================
    // QUALITY
    // =========================

    void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();

        qualityDropdown.AddOptions(
            new System.Collections.Generic.List<string>(QualitySettings.names)
        );
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

        fpsDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Unlimited",
            "30",
            "60",
            "120",
            "144",
            "240",
            
        });
    }

    public void SetFPSLimit(int index)
    {
        switch (index)
        {

            case 0:
                Application.targetFrameRate = -1;
                break;

            case 1:
                Application.targetFrameRate = 30;
                break;

            case 2:
                Application.targetFrameRate = 60;
                break;

            case 3:
                Application.targetFrameRate = 120;
                break;

            case 4:
                Application.targetFrameRate = 144;
                break;

            case 5:
                Application.targetFrameRate = 240;
                break;

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
        int resolutionIndex =
            PlayerPrefs.GetInt("Resolution", resolutionDropdown.value);

        resolutionDropdown.value = resolutionIndex;
        SetResolution(resolutionIndex);

        // Quality
        int qualityIndex =
            PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());

        qualityDropdown.value = qualityIndex;
        SetQuality(qualityIndex);

        // Volume
        float volume = PlayerPrefs.GetFloat("Volume", 1f);

        volumeSlider.value = volume;
        SetVolume(volume);

        // FPS
        int fpsIndex = PlayerPrefs.GetInt("FPS", 1);

        fpsDropdown.value = fpsIndex;
        SetFPSLimit(fpsIndex);
    }
}