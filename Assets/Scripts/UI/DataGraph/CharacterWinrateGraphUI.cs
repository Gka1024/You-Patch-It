
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterWinRateGraphUI : MonoBehaviour
{
    private struct GraphPoint
    {
        public int season;
        public int subSeason;

        public GraphPoint(int season, int subSeason = 0)
        {
            this.season = season;
            this.subSeason = subSeason;
        }

        public string Label => subSeason > 0 ? $"S{season}-{subSeason}" : $"S{season}";
    }

    [Header("Stat Toggles")]
    [SerializeField] private Toggle winRateToggle;
    [SerializeField] private Toggle pickRateToggle;
    [SerializeField] private Toggle subSeasonToggle;

    [Header("Season Selection")]
    [SerializeField] private TMP_Dropdown seasonStartDropdown;
    [SerializeField] private TMP_Dropdown seasonEndDropdown;

    [Header("Graph")]
    [SerializeField] private RectTransform graphArea;
    [SerializeField] private Image linePrefab;
    [SerializeField] private Image pointPrefab;
    [SerializeField] private Image gridLinePrefab;
    [SerializeField] private Image xAxisPrefab;
    [SerializeField] private Image yAxisPrefab;

    [Header("Axis Labels")]
    [SerializeField] private TMP_Text axisLabelPrefab;
    [SerializeField] private TMP_Text seasonLabelPrefab;
    [SerializeField] private float axisFontSize = 14f;
    [SerializeField] private Color axisLabelColor = Color.white;

    [Header("Graph Layout")]
    [SerializeField] private float leftPadding = 55f;
    [SerializeField] private float rightPadding = 55f;
    [SerializeField] private float topPadding = 15f;
    [SerializeField] private float bottomPadding = 45f;
    [SerializeField] private float lineThickness = 3f;
    [SerializeField] private float pointSize = 10f;
    [SerializeField] private float seasonSpacing = 100f;
    [SerializeField] private float axisThickness = 2f;
    [SerializeField] private float gridThickness = 1f;

    [Header("Win Rate Y Axis")]
    [SerializeField] private float defaultMinWinRate = 30f;
    [SerializeField] private float defaultMaxWinRate = 70f;

    [Header("Pick Rate Y Axis")]
    [SerializeField] private float defaultMinPickRate = 0f;
    [SerializeField] private float defaultMaxPickRate = 20f;

    [Header("Y Axis")]
    [SerializeField] private float yAxisStep = 10f;
    [SerializeField] private int yAxisDivisionCount = 4;

    [Header("Character Toggles")]
    [SerializeField] private CharacterToggleUI characterTogglePrefab;
    [SerializeField] private Transform characterToggleParent;

    [Header("Character Colors")]
    [SerializeField] private Color[] seriesColors =
    {
        new Color(0.3f, 0.9f, 0.55f),
        new Color(1f, 0.65f, 0.25f),
        new Color(0.35f, 0.7f, 1f),
        new Color(0.9f, 0.4f, 0.7f),
        new Color(0.8f, 0.8f, 0.35f)
    };

    private readonly List<RuntimeCharacter> selectedCharacters = new();
    private readonly List<int> availableSeasons = new();
    private readonly List<GameObject> generatedGraphObjects = new();
    private readonly List<CharacterToggleUI> generatedToggleObjects = new();
    private readonly Dictionary<int, bool> characterVisibility = new();

    private int firstSeason = 1;
    private int lastSeason = 5;

    private bool showWinRate = true;
    private bool showPickRate;
    private bool showSubSeason = true;
    private bool initialized;

    private void Awake()
    {
        if (winRateToggle != null)
            winRateToggle.onValueChanged.AddListener(HandleWinRateToggleChanged);

        if (pickRateToggle != null)
            pickRateToggle.onValueChanged.AddListener(HandlePickRateToggleChanged);

        if (subSeasonToggle != null)
            subSeasonToggle.onValueChanged.AddListener(HandleSubSeasonToggleChanged);

        if (seasonStartDropdown != null)
            seasonStartDropdown.onValueChanged.AddListener(HandleStartSeasonChanged);

        if (seasonEndDropdown != null)
            seasonEndDropdown.onValueChanged.AddListener(HandleEndSeasonChanged);
    }

    private void Start()
    {
        if (winRateToggle != null)
            showWinRate = winRateToggle.isOn;

        if (pickRateToggle != null)
            showPickRate = pickRateToggle.isOn;

        if (subSeasonToggle != null)
            showSubSeason = subSeasonToggle.isOn;

        RefreshAvailableSeasons();

        if (availableSeasons.Count == 0)
        {
            for (int season = 1; season <= 5; season++)
                availableSeasons.Add(season);
        }

        RefreshSeasonDropdowns();

        if (RuntimeCharacterManager.Instance != null)
            SetCharacters(RuntimeCharacterManager.Instance.GetAllCharacters().ToList());

        initialized = true;
        RebuildGraph();
    }

    private void OnEnable()
    {
        if (!initialized)
            return;

        RefreshAvailableSeasons();

        if (RuntimeCharacterManager.Instance != null)
            SetCharacters(RuntimeCharacterManager.Instance.GetAllCharacters().ToList());

        RebuildGraph();
    }

    private void OnDestroy()
    {
        if (winRateToggle != null)
            winRateToggle.onValueChanged.RemoveListener(HandleWinRateToggleChanged);

        if (pickRateToggle != null)
            pickRateToggle.onValueChanged.RemoveListener(HandlePickRateToggleChanged);

        if (subSeasonToggle != null)
            subSeasonToggle.onValueChanged.RemoveListener(HandleSubSeasonToggleChanged);

        if (seasonStartDropdown != null)
            seasonStartDropdown.onValueChanged.RemoveListener(HandleStartSeasonChanged);

        if (seasonEndDropdown != null)
            seasonEndDropdown.onValueChanged.RemoveListener(HandleEndSeasonChanged);

        foreach (CharacterToggleUI toggle in generatedToggleObjects)
        {
            if (toggle != null)
                toggle.OnToggleChanged -= HandleCharacterToggleChanged;
        }
    }

    private void HandleWinRateToggleChanged(bool isOn)
    {
        showWinRate = isOn;
        RebuildGraph();
    }

    private void HandlePickRateToggleChanged(bool isOn)
    {
        showPickRate = isOn;
        RebuildGraph();
    }

    private void HandleSubSeasonToggleChanged(bool isOn)
    {
        showSubSeason = isOn;
        RebuildGraph();
    }

    public void SetAvailableSeasons()
    {
        if (SeasonManager.Instance != null)
            SetAvailableSeasons(SeasonManager.Instance.CurrentSeason);
    }

    public void SetAvailableSeasons(int currentSeason)
    {
        availableSeasons.Clear();

        for (int season = 1; season < currentSeason; season++)
            availableSeasons.Add(season);

        RefreshSeasonDropdowns();
        RebuildGraph();
    }

    private void RefreshAvailableSeasons()
    {
        if (AnalysisManager.Instance == null)
            return;

        List<int> seasons = AnalysisManager.Instance.GetAvailableSeasons();

        if (seasons == null || seasons.Count == 0)
            return;

        availableSeasons.Clear();
        availableSeasons.AddRange(seasons.Distinct().OrderBy(x => x));

        RefreshSeasonDropdowns();
    }

    public void SetCharacters(List<RuntimeCharacter> characters)
    {
        selectedCharacters.Clear();

        if (characters != null)
        {
            foreach (RuntimeCharacter character in characters)
            {
                if (character == null || character.OriginCharacter == null)
                    continue;

                int characterId = character.OriginCharacter.id;

                if (selectedCharacters.Any(x => x.OriginCharacter.id == characterId))
                    continue;

                selectedCharacters.Add(character);

                if (!characterVisibility.ContainsKey(characterId))
                    characterVisibility[characterId] = true;
            }
        }

        CreateCharacterToggles();
        RebuildGraph();
    }

    private void RefreshSeasonDropdowns()
    {
        if (availableSeasons.Count == 0)
        {
            if (seasonStartDropdown != null)
                seasonStartDropdown.ClearOptions();

            if (seasonEndDropdown != null)
                seasonEndDropdown.ClearOptions();

            return;
        }

        List<string> options = availableSeasons
            .Select(season => $"Season {season}")
            .ToList();

        if (seasonStartDropdown != null)
        {
            seasonStartDropdown.ClearOptions();
            seasonStartDropdown.AddOptions(options);
        }

        if (seasonEndDropdown != null)
        {
            seasonEndDropdown.ClearOptions();
            seasonEndDropdown.AddOptions(options);
        }

        int startIndex = availableSeasons.IndexOf(firstSeason);
        int endIndex = availableSeasons.IndexOf(lastSeason);

        if (startIndex < 0)
            startIndex = 0;

        if (endIndex < 0)
            endIndex = availableSeasons.Count - 1;

        if (startIndex > endIndex)
            startIndex = endIndex;

        firstSeason = availableSeasons[startIndex];
        lastSeason = availableSeasons[endIndex];

        if (seasonStartDropdown != null)
        {
            seasonStartDropdown.SetValueWithoutNotify(startIndex);
            seasonStartDropdown.RefreshShownValue();
        }

        if (seasonEndDropdown != null)
        {
            seasonEndDropdown.SetValueWithoutNotify(endIndex);
            seasonEndDropdown.RefreshShownValue();
        }
    }

    private void HandleStartSeasonChanged(int index)
    {
        if (index < 0 || index >= availableSeasons.Count)
            return;

        firstSeason = availableSeasons[index];

        int endIndex = availableSeasons.IndexOf(lastSeason);

        if (index > endIndex)
        {
            lastSeason = firstSeason;

            if (seasonEndDropdown != null)
            {
                seasonEndDropdown.SetValueWithoutNotify(index);
                seasonEndDropdown.RefreshShownValue();
            }
        }

        RebuildGraph();
    }

    private void HandleEndSeasonChanged(int index)
    {
        if (index < 0 || index >= availableSeasons.Count)
            return;

        lastSeason = availableSeasons[index];

        int startIndex = availableSeasons.IndexOf(firstSeason);

        if (index < startIndex)
        {
            firstSeason = lastSeason;

            if (seasonStartDropdown != null)
            {
                seasonStartDropdown.SetValueWithoutNotify(index);
                seasonStartDropdown.RefreshShownValue();
            }
        }

        RebuildGraph();
    }

    private void CreateCharacterToggles()
    {
        foreach (CharacterToggleUI toggle in generatedToggleObjects)
        {
            if (toggle == null)
                continue;

            toggle.OnToggleChanged -= HandleCharacterToggleChanged;
            Destroy(toggle.gameObject);
        }

        generatedToggleObjects.Clear();

        if (characterTogglePrefab == null || characterToggleParent == null)
            return;

        for (int i = 0; i < selectedCharacters.Count; i++)
        {
            RuntimeCharacter character = selectedCharacters[i];

            if (character == null || character.OriginCharacter == null)
                continue;

            CharacterToggleUI toggle = Instantiate(characterTogglePrefab, characterToggleParent);
            toggle.Initialize(character, GetSeriesColor(i));

            int characterId = character.OriginCharacter.id;
            bool isVisible = !characterVisibility.TryGetValue(characterId, out bool visible) || visible;

            toggle.SetIsOnWithoutNotify(isVisible);
            toggle.OnToggleChanged += HandleCharacterToggleChanged;

            generatedToggleObjects.Add(toggle);
        }
    }

    private void HandleCharacterToggleChanged(RuntimeCharacter character, bool isOn)
    {
        if (character == null || character.OriginCharacter == null)
            return;

        characterVisibility[character.OriginCharacter.id] = isOn;
        RebuildGraph();
    }

    public void RebuildGraph()
    {
        ClearGeneratedGraphObjects();

        if (graphArea == null || AnalysisManager.Instance == null || availableSeasons.Count == 0)
            return;

        Canvas.ForceUpdateCanvases();

        Rect rect = graphArea.rect;

        if (rect.width <= 0f || rect.height <= 0f)
            return;

        List<GraphPoint> points = BuildGraphPoints();

        if (points.Count == 0)
            return;

        float plotWidth = rect.width - leftPadding - rightPadding;
        float plotHeight = rect.height - topPadding - bottomPadding;

        if (plotWidth <= 0f || plotHeight <= 0f)
            return;

        float spacing = points.Count > 1
            ? Mathf.Min(seasonSpacing, plotWidth / (points.Count - 1))
            : 0f;

        float usedWidth = spacing * (points.Count - 1);
        float startX = -rect.width / 2f + leftPadding + (plotWidth - usedWidth) / 2f;

        Dictionary<int, List<float>> winRateValues = new();
        Dictionary<int, List<float>> pickRateValues = new();

        float winMin = defaultMinWinRate;
        float winMax = defaultMaxWinRate;
        float pickMin = defaultMinPickRate;
        float pickMax = defaultMaxPickRate;

        foreach (RuntimeCharacter character in selectedCharacters)
        {
            if (character == null || character.OriginCharacter == null)
                continue;

            int characterId = character.OriginCharacter.id;

            if (!IsCharacterVisible(characterId))
                continue;

            List<float> winRates = new();
            List<float> pickRates = new();

            foreach (GraphPoint point in points)
            {
                float winRate = GetWinRate(characterId, point);
                float pickRate = GetPickRate(characterId, point);

                winRates.Add(winRate);
                pickRates.Add(pickRate);

                if (winRate >= 0f)
                {
                    winMin = Mathf.Min(winMin, winRate);
                    winMax = Mathf.Max(winMax, winRate);
                }

                if (pickRate >= 0f)
                {
                    pickMin = Mathf.Min(pickMin, pickRate);
                    pickMax = Mathf.Max(pickMax, pickRate);
                }
            }

            winRateValues[characterId] = winRates;
            pickRateValues[characterId] = pickRates;
        }

        float step = Mathf.Max(1f, yAxisStep);

        winMin = Mathf.Max(0f, Mathf.Floor(winMin / step) * step);
        winMax = Mathf.Min(100f, Mathf.Ceil(winMax / step) * step);

        pickMin = Mathf.Max(0f, Mathf.Floor(pickMin / step) * step);
        pickMax = Mathf.Min(100f, Mathf.Ceil(pickMax / step) * step);

        if (winMax <= winMin)
            winMax = Mathf.Min(100f, winMin + step);

        if (pickMax <= pickMin)
            pickMax = Mathf.Min(100f, pickMin + step);

        DrawAxesAndGrid(rect, plotWidth, plotHeight, startX, spacing, points,
            winMin, winMax, pickMin, pickMax);

        for (int i = 0; i < selectedCharacters.Count; i++)
        {
            RuntimeCharacter character = selectedCharacters[i];

            if (character == null || character.OriginCharacter == null)
                continue;

            int characterId = character.OriginCharacter.id;

            if (showWinRate && winRateValues.TryGetValue(characterId, out List<float> winRates))
            {
                DrawCharacterGraph(character, i, points, winRates, startX, spacing,
                    plotHeight, winMin, winMax, rect.height, false);
            }

            if (showPickRate && pickRateValues.TryGetValue(characterId, out List<float> pickRates))
            {
                DrawCharacterGraph(character, i, points, pickRates, startX, spacing,
                    plotHeight, pickMin, pickMax, rect.height, true);
            }
        }
    }

    private List<GraphPoint> BuildGraphPoints()
    {
        List<GraphPoint> points = new();

        foreach (int season in availableSeasons.Where(x => x >= firstSeason && x <= lastSeason).OrderBy(x => x))
        {
            if (showSubSeason)
            {
                List<int> subSeasons = AnalysisManager.Instance.GetAvailableSubSeasons(season);

                if (subSeasons == null)
                    continue;

                foreach (int subSeason in subSeasons.Distinct().OrderBy(x => x))
                    points.Add(new GraphPoint(season, subSeason));
            }
            else
            {
                points.Add(new GraphPoint(season));
            }
        }

        return points;
    }

    private float GetWinRate(int characterId, GraphPoint point)
    {
        if (point.subSeason > 0)
            return AnalysisManager.Instance.GetSubSeasonWinRate(characterId, point.season, point.subSeason);

        return AnalysisManager.Instance.GetSeasonWinRate(characterId, point.season);
    }

    private float GetPickRate(int characterId, GraphPoint point)
    {
        float pickRate = point.subSeason > 0
            ? AnalysisManager.Instance.GetSubSeasonPickRate(characterId, point.season, point.subSeason)
            : AnalysisManager.Instance.GetSeasonPickRate(characterId, point.season);

        // 픽 기록이 없는 캐릭터는 0% 지점에 그리지 않고 데이터 없음으로 처리한다.
        return pickRate <= 0f ? -1f : pickRate;
    }

    private void DrawAxesAndGrid(Rect rect, float plotWidth, float plotHeight, float startX, float spacing, List<GraphPoint> points, float winMin, float winMax, float pickMin, float pickMax)
    {
        float leftX = -rect.width / 2f + leftPadding;
        float rightX = leftX + plotWidth;
        float bottomY = -rect.height / 2f + bottomPadding;
        float topY = bottomY + plotHeight;

        CreateAxisLine(xAxisPrefab, "XAxis", new Vector2(leftX, bottomY), new Vector2(rightX, bottomY), axisThickness);

        if (showWinRate)
            CreateAxisLine(yAxisPrefab, "YAxisWinRate", new Vector2(leftX, bottomY), new Vector2(leftX, topY), axisThickness);

        if (showPickRate)
            CreateAxisLine(yAxisPrefab, "YAxisPickRate", new Vector2(rightX, bottomY), new Vector2(rightX, topY), axisThickness);

        int divisions = Mathf.Max(1, yAxisDivisionCount);

        for (int i = 0; i <= divisions; i++)
        {
            float normalizedY = (float)i / divisions;
            float y = bottomY + normalizedY * plotHeight;

            Image grid = CreateImage(
                gridLinePrefab != null ? gridLinePrefab : linePrefab,
                $"Grid_{i}",
                new Color(1f, 1f, 1f, 0.18f));

            if (grid != null)
                SetLineTransform(grid.rectTransform, new Vector2(leftX, y), new Vector2(rightX, y), gridThickness);

            if (showWinRate)
            {
                float winValue = Mathf.Lerp(winMin, winMax, normalizedY);

                CreateAxisLabel(
                    $"{winValue:0}%",
                    new Vector2(leftX - 8f, y),
                    TextAlignmentOptions.MidlineRight,
                    new Vector2(50f, 24f));
            }

            if (showPickRate)
            {
                float pickValue = Mathf.Lerp(pickMin, pickMax, normalizedY);

                CreateAxisLabel(
                    $"{pickValue:0}%",
                    new Vector2(rightX + 8f, y),
                    TextAlignmentOptions.MidlineLeft,
                    new Vector2(50f, 24f));
            }
        }

        for (int i = 0; i < points.Count; i++)
        {
            float x = startX + i * spacing;
            CreateSeasonLabel(points[i].Label, new Vector2(x, bottomY - 18f));
        }
    }

    private void DrawCharacterGraph(RuntimeCharacter character, int colorIndex, List<GraphPoint> points, List<float> values, float startX, float spacing, float plotHeight, float yMin, float yMax, float graphHeight, bool isPickRate)
    {
        Color color = GetSeriesColor(colorIndex);

        if (isPickRate)
            color = Color.Lerp(color, Color.white, 0.45f);

        float bottomY = -graphHeight / 2f + bottomPadding;
        Vector2? previousPoint = null;

        for (int i = 0; i < points.Count; i++)
        {
            float value = values[i];

            if (value < 0f)
            {
                previousPoint = null;
                continue;
            }

            float x = startX + i * spacing;
            float y = GetGraphY(value, bottomY, plotHeight, yMin, yMax);
            Vector2 currentPoint = new(x, y);

            string metricSuffix = isPickRate ? "PickRate" : "WinRate";

            if (previousPoint.HasValue)
            {
                Image line = CreateImage(
                    linePrefab,
                    $"Line_{character.OriginCharacter.id}_{points[i].Label}_{metricSuffix}",
                    color);

                if (line != null)
                    SetLineTransform(line.rectTransform, previousPoint.Value, currentPoint, lineThickness);
            }

            Image point = CreateImage(
                pointPrefab,
                $"Point_{character.OriginCharacter.id}_{points[i].Label}_{metricSuffix}",
                color);

            if (point != null)
                SetPointTransform(point.rectTransform, currentPoint, pointSize);

            previousPoint = currentPoint;
        }
    }

    private bool IsCharacterVisible(int characterId)
    {
        return !characterVisibility.TryGetValue(characterId, out bool visible) || visible;
    }

    private float GetGraphY(float value, float bottomY, float plotHeight, float yMin, float yMax)
    {
        return bottomY + Mathf.InverseLerp(yMin, yMax, value) * plotHeight;
    }

    private void CreateAxisLabel(string text, Vector2 position, TextAlignmentOptions alignment, Vector2 size)
    {
        TMP_Text label;

        if (axisLabelPrefab != null)
        {
            label = Instantiate(axisLabelPrefab, graphArea);
        }
        else
        {
            GameObject obj = new($"AxisLabel_{text}", typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(graphArea, false);
            label = obj.GetComponent<TextMeshProUGUI>();
        }

        label.text = text;
        label.fontSize = axisFontSize;
        label.color = axisLabelColor;
        label.alignment = alignment;
        label.raycastTarget = false;

        SetCenteredRect(label.rectTransform, position, size);
        generatedGraphObjects.Add(label.gameObject);
    }

    private void CreateSeasonLabel(string text, Vector2 position)
    {
        TMP_Text label;

        if (seasonLabelPrefab != null)
        {
            label = Instantiate(seasonLabelPrefab, graphArea);
        }
        else if (axisLabelPrefab != null)
        {
            label = Instantiate(axisLabelPrefab, graphArea);
        }
        else
        {
            GameObject obj = new($"SeasonLabel_{text}", typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(graphArea, false);
            label = obj.GetComponent<TextMeshProUGUI>();
        }

        label.text = text;
        label.fontSize = axisFontSize;
        label.color = axisLabelColor;
        label.alignment = TextAlignmentOptions.Top;
        label.raycastTarget = false;

        SetCenteredRect(label.rectTransform, position, new Vector2(60f, 24f));
        generatedGraphObjects.Add(label.gameObject);
    }

    private void CreateAxisLine(Image prefab, string objectName, Vector2 start, Vector2 end, float thickness)
    {
        if (prefab == null)
            return;

        Image axis = CreateImage(prefab, objectName, Color.white);

        if (axis != null)
            SetLineTransform(axis.rectTransform, start, end, thickness);
    }

    private Image CreateImage(Image prefab, string objectName, Color color)
    {
        Image image;

        if (prefab != null)
        {
            image = Instantiate(prefab, graphArea);
        }
        else
        {
            GameObject obj = new(objectName, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(graphArea, false);
            image = obj.GetComponent<Image>();
        }

        image.name = objectName;
        image.color = color;
        image.raycastTarget = false;

        SetCenteredRect(image.rectTransform, Vector2.zero, Vector2.one);
        generatedGraphObjects.Add(image.gameObject);

        return image;
    }

    private void SetLineTransform(RectTransform rt, Vector2 start, Vector2 end, float thickness)
    {
        Vector2 direction = end - start;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = (start + end) * 0.5f;
        rt.sizeDelta = new Vector2(direction.magnitude, thickness);
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void SetPointTransform(RectTransform rt, Vector2 position, float size)
    {
        SetCenteredRect(rt, position, new Vector2(size, size));
        rt.localRotation = Quaternion.identity;
    }

    private void SetCenteredRect(RectTransform rt, Vector2 position, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        rt.localRotation = Quaternion.identity;
    }

    private Color GetSeriesColor(int index)
    {
        if (seriesColors == null || seriesColors.Length == 0)
            return Color.white;

        return seriesColors[index % seriesColors.Length];
    }

    private void ClearGeneratedGraphObjects()
    {
        foreach (GameObject obj in generatedGraphObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        generatedGraphObjects.Clear();
    }
}
