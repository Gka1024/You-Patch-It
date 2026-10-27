using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PreviewCameraScaler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera previewCamera;

    [Header("Reference Settings")]
    [SerializeField] private float baseOrthographicSize = 220f;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    private float previousAspect;
    private int previousHeight;

    private void Awake()
    {
        if (previewCamera == null)
            previewCamera = GetComponent<Camera>();

        previewCamera.orthographic = true;
        UpdateCameraScale();
    }

    private void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        if (!Mathf.Approximately(currentAspect, previousAspect) || Screen.height != previousHeight)
            UpdateCameraScale();
    }

    private void UpdateCameraScale()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceResolution.x / referenceResolution.y;

        // 화면 비율이 좁아질 때 가로 방향 잘림 방지
        float aspectScale = referenceAspect / currentAspect;
        aspectScale = Mathf.Max(1f, aspectScale);

        // 기준 해상도 대비 현재 해상도의 세로 배율
        float resolutionScale = (float)Screen.height / referenceResolution.y;

        // 해상도와 화면 비율을 함께 반영
        previewCamera.orthographicSize = baseOrthographicSize * resolutionScale * aspectScale;

        previousAspect = currentAspect;
        previousHeight = Screen.height;
    }
}
