using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterWinRateGraphUI : MonoBehaviour
{
    [Header("Graph")]
    [SerializeField] private RectTransform graphArea;
    [SerializeField] private Image linePrefab;
    [SerializeField] private Image pointPrefab;
    [SerializeField] private Image gridLinePrefab;

    [Header("Season Range")]
    [SerializeField] private int firstSeason = 1;
    [SerializeField] private int lastSeason = 5;

    [Header("Graph Layout")]
    [SerializeField] private float leftPadding = 45f;
    [SerializeField] private float rightPadding = 20f;
    [SerializeField] private float topPadding = 15f;
    [SerializeField] private float bottomPadding = 35f;
    [SerializeField] private float lineThickness = 3f;
    [SerializeField] private float pointSize = 10f;

    [Header("Axis Labels")]
    [SerializeField] private TMP_Text axisLabelPrefab;
    [SerializeField] private float axisFontSize = 14f;

    [Header("Legend")]
    [SerializeField] private Transform legendParent;
    [SerializeField] private TMP_Text legendLabelPrefab;

    [Header("Character Colors")]
    [SerializeField]
    private Color[] seriesColors =
    {
        new Color(0.3f, 0.9f, 0.55f),
        new Color(1f, 0.65f, 0.25f),
        new Color(0.35f, 0.7f, 1f)
    };

    private readonly List<RuntimeCharacter> selectedCharacters = new();
    private readonly List<GameObject> generatedObjects = new();

    private void Start()
    {
        RebuildGraph();
        SetCharacters(RuntimeCharacterManager.Instance.GetAllCharacters().ToList());
    }

    void OnEnable()
    {
       Start();
    }

    public void SetCharacters(List<RuntimeCharacter> characters)
    {
        selectedCharacters.Clear();

        if (characters != null)
        {
            foreach (RuntimeCharacter character in characters)
            {
                if (character == null || selectedCharacters.Contains(character))
                    continue;

                selectedCharacters.Add(character);

                if (selectedCharacters.Count >= 3)
                    break;
            }
        }

        RebuildGraph();
    }

    public void SetSeasonRange(int startSeason, int endSeason)
    {
        firstSeason = Mathf.Max(1, startSeason);
        lastSeason = Mathf.Max(firstSeason, endSeason);

        RebuildGraph();
    }

    public void RebuildGraph()
    {
        Debug.Log("RebuildGraph 실행");

        ClearGeneratedObjects();

        if (graphArea == null)
        {
            Debug.LogError("GraphArea가 연결되지 않았습니다.");
            return;
        }

        Debug.Log($"GraphArea 크기: {graphArea.rect.size}");

        DrawGrid();

        if (AnalysisManager.Instance == null)
        {
            Debug.LogError("AnalysisManager.Instance가 null입니다.");
            return;
        }

        Debug.Log($"선택된 캐릭터 수: {selectedCharacters.Count}");

        for (int i = 0; i < selectedCharacters.Count; i++)
        {
            DrawCharacterGraph(selectedCharacters[i], i);
        }
    }

    private void DrawGrid()
    {
        Rect rect = graphArea.rect;

        float plotWidth = rect.width - leftPadding - rightPadding;
        float plotHeight = rect.height - topPadding - bottomPadding;

        if (plotWidth <= 0f || plotHeight <= 0f)
            return;

        for (int rate = 0; rate <= 100; rate += 25)
        {
            float y = GetGraphY(rate, plotHeight, rect.height);

            Image gridLine = CreateImage(
                gridLinePrefab != null ? gridLinePrefab : linePrefab,
                graphArea,
                $"GridLine_{rate}",
                new Color(1f, 1f, 1f, 0.15f));

            SetLineTransform(
                gridLine.rectTransform,
                new Vector2(-rect.width / 2f + leftPadding, y),
                new Vector2(-rect.width / 2f + leftPadding + plotWidth, y),
                1f);

            CreateAxisLabel(
                $"{rate}%",
                new Vector2(-rect.width / 2f + leftPadding - 8f, y),
                TextAlignmentOptions.MidlineRight);
        }

        int seasonCount = lastSeason - firstSeason + 1;

        for (int i = 0; i < seasonCount; i++)
        {
            int season = firstSeason + i;
            float normalizedX = seasonCount <= 1 ? 0.5f : (float)i / (seasonCount - 1);
            float x = -rect.width / 2f + leftPadding + normalizedX * plotWidth;
            float y = -rect.height / 2f + bottomPadding - 16f;

            CreateAxisLabel(
                $"S{season}",
                new Vector2(x, y),
                TextAlignmentOptions.Top);
        }
    }

    private void DrawCharacterGraph(RuntimeCharacter character, int colorIndex)
    {
        if (character == null)
            return;

        Rect rect = graphArea.rect;

        float plotWidth = rect.width - leftPadding - rightPadding;
        float plotHeight = rect.height - topPadding - bottomPadding;

        if (plotWidth <= 0f || plotHeight <= 0f)
            return;

        Color color = GetSeriesColor(colorIndex);
        Vector2? previousPoint = null;

        int seasonCount = lastSeason - firstSeason + 1;

        for (int i = 0; i < seasonCount; i++)
        {
            int season = firstSeason + i;
            float winRate = AnalysisManager.Instance.GetSeasonWinRate(
                character.OriginCharacter.id,
                season);

            if (winRate < 0f)
            {
                previousPoint = null;
                continue;
            }

            float normalizedX = seasonCount <= 1 ? 0.5f : (float)i / (seasonCount - 1);

            float x = -rect.width / 2f + leftPadding + normalizedX * plotWidth;
            float y = GetGraphY(winRate, plotHeight, rect.height);

            Vector2 currentPoint = new(x, y);

            if (previousPoint.HasValue)
            {
                Image line = CreateImage(
                    linePrefab,
                    graphArea,
                    $"WinRateLine_{character.OriginCharacter.id}_{season}",
                    color);

                SetLineTransform(line.rectTransform, previousPoint.Value, currentPoint, lineThickness);
            }

            Image point = CreateImage(
                pointPrefab,
                graphArea,
                $"WinRatePoint_{character.OriginCharacter.id}_{season}",
                color);

            SetPointTransform(point.rectTransform, currentPoint, pointSize);

            previousPoint = currentPoint;
        }

        CreateLegendLabel(character.OriginCharacter.characterName, colorIndex);
    }

    private float GetGraphY(float winRate, float plotHeight, float graphHeight)
    {
        float normalizedY = Mathf.Clamp01(winRate / 100f);

        return -graphHeight / 2f + bottomPadding + normalizedY * plotHeight;
    }

    private void SetLineTransform(RectTransform rectTransform, Vector2 start, Vector2 end, float thickness)
    {
        Vector2 direction = end - start;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = (start + end) * 0.5f;
        rectTransform.sizeDelta = new Vector2(direction.magnitude, thickness);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void SetPointTransform(RectTransform rectTransform, Vector2 position, float size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(size, size);
        rectTransform.localRotation = Quaternion.identity;
    }

    private Image CreateImage(Image prefab, Transform parent, string objectName, Color color)
    {
        Image image;

        if (prefab != null)
        {
            image = Instantiate(prefab, parent);
        }
        else
        {
            GameObject obj = new(objectName, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            image = obj.GetComponent<Image>();
        }

        image.name = objectName;
        image.color = color;
        image.raycastTarget = false;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        generatedObjects.Add(image.gameObject);

        return image;
    }

    private void CreateAxisLabel(string text, Vector2 position, TextAlignmentOptions alignment)
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
        label.color = Color.white;
        label.alignment = alignment;
        label.raycastTarget = false;

        RectTransform rectTransform = label.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(50f, 24f);

        generatedObjects.Add(label.gameObject);
    }

    private void CreateLegendLabel(string characterName, int colorIndex)
    {
        if (legendParent == null)
            return;

        TMP_Text label;

        if (legendLabelPrefab != null)
        {
            label = Instantiate(legendLabelPrefab, legendParent);
        }
        else
        {
            GameObject obj = new($"Legend_{characterName}", typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(legendParent, false);
            label = obj.GetComponent<TextMeshProUGUI>();
        }

        label.text = characterName;
        label.color = GetSeriesColor(colorIndex);
        label.raycastTarget = false;

        generatedObjects.Add(label.gameObject);
    }

    private Color GetSeriesColor(int index)
    {
        if (seriesColors == null || seriesColors.Length == 0)
            return Color.white;

        return seriesColors[index % seriesColors.Length];
    }

    private void ClearGeneratedObjects()
    {
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        generatedObjects.Clear();
    }
}
