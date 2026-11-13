using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    public static ResolutionManager Instance { get; private set; }

    private const string WIDTH_KEY = "ResolutionWidth";
    private const string HEIGHT_KEY = "ResolutionHeight";
    private const string FULLSCREEN_KEY = "FullScreenMode";
    private const string HAS_SETTINGS_KEY = "HasResolutionSettings";

    [Header("Settings UI")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown fullScreenDropdown;

    private readonly List<Resolution> resolutions = new();

    public int CurrentWidth => Screen.width;
    public int CurrentHeight => Screen.height;
    public FullScreenMode CurrentFullScreenMode => Screen.fullScreenMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeResolutions();
        InitializeSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void InitializeResolutions()
    {
        resolutions.Clear();

        Resolution[] supportedResolutions = Screen.resolutions;

        foreach (Resolution resolution in supportedResolutions)
        {
            bool exists = resolutions.Exists(value =>
                value.width == resolution.width &&
                value.height == resolution.height);

            if (!exists)
                resolutions.Add(resolution);
        }

        resolutions.Sort((a, b) =>
        {
            int widthCompare = a.width.CompareTo(b.width);
            return widthCompare != 0 ? widthCompare : a.height.CompareTo(b.height);
        });

        if (resolutionDropdown != null)
        {
            List<string> options = new();

            foreach (Resolution resolution in resolutions)
                options.Add($"{resolution.width} x {resolution.height}");

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
        }
    }

    private void InitializeSettings()
    {
        if (PlayerPrefs.GetInt(HAS_SETTINGS_KEY, 0) == 1)
        {
            int width = PlayerPrefs.GetInt(WIDTH_KEY);
            int height = PlayerPrefs.GetInt(HEIGHT_KEY);
            int mode = PlayerPrefs.GetInt(FULLSCREEN_KEY);

            SetResolution(width, height, (FullScreenMode)mode);
        }
        else
        {
            // 최초 실행 시 현재 모니터 해상도를 사용한다.
            SetResolution(
                Display.main.systemWidth,
                Display.main.systemHeight,
                FullScreenMode.FullScreenWindow
            );
        }

        RefreshSettingsUI();
    }

    public void SetResolution(int width, int height, FullScreenMode mode)
    {
        Screen.SetResolution(width, height, mode);

        PlayerPrefs.SetInt(WIDTH_KEY, width);
        PlayerPrefs.SetInt(HEIGHT_KEY, height);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, (int)mode);
        PlayerPrefs.SetInt(HAS_SETTINGS_KEY, 1);
        PlayerPrefs.Save();

        RefreshSettingsUI();
    }

    public void SetResolutionByIndex(int index)
    {
        if (index < 0 || index >= resolutions.Count)
            return;

        Resolution resolution = resolutions[index];

        SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    public void SetFullScreenMode(int index)
    {
        if (index < 0 || index > 2)
            return;

        FullScreenMode mode = index switch
        {
            0 => FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.ExclusiveFullScreen,
            _ => FullScreenMode.FullScreenWindow
        };

        SetResolution(Screen.width, Screen.height, mode);
    }

    private void RefreshSettingsUI()
    {
        if (resolutionDropdown != null)
        {
            int index = resolutions.FindIndex(value =>
                value.width == Screen.width &&
                value.height == Screen.height);

            if (index >= 0)
                resolutionDropdown.SetValueWithoutNotify(index);
        }

        if (fullScreenDropdown != null)
        {
            int index = Screen.fullScreenMode switch
            {
                FullScreenMode.FullScreenWindow => 0,
                FullScreenMode.Windowed => 1,
                FullScreenMode.ExclusiveFullScreen => 2,
                _ => 0
            };

            fullScreenDropdown.SetValueWithoutNotify(index);
        }
    }
}