using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionManager : MonoBehaviour
{
    [Header("Settings UI")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullScreenToggle;

    private readonly List<Resolution> resolutions = new();

    private void Start()
    {
        InitializeResolutions();

        fullScreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        fullScreenToggle.onValueChanged.AddListener(SetFullScreen);

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void InitializeResolutions()
    {
        resolutions.Clear();

        Resolution[] allResolutions = Screen.resolutions;
        List<string> options = new();

        int currentResolutionIndex = 0;

        foreach (Resolution resolution in allResolutions)
        {
            bool exists = resolutions.Exists(value =>
                value.width == resolution.width &&
                value.height == resolution.height);

            if (exists)
                continue;

            resolutions.Add(resolution);
            options.Add($"{resolution.width} x {resolution.height}");

            if (resolution.width == Screen.width &&
                resolution.height == Screen.height)
            {
                currentResolutionIndex = resolutions.Count - 1;
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= resolutions.Count)
            return;

        Resolution resolution = resolutions[resolutionIndex];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            Screen.fullScreen
        );

        Debug.Log($"해상도 변경: {resolution.width}x{resolution.height}");
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;

        Debug.Log($"화면 모드 변경: {(isFullScreen ? "전체 화면" : "창 모드")}");
    }
}