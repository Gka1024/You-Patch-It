using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class InspectorLongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private float repeatDelay = 0.4f;
    [SerializeField] private float repeatInterval = 0.1f;

    private Coroutine repeatCoroutine;
    private bool isPointerDown;
    private bool hasRepeated;

    public System.Action onClick;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        hasRepeated = false;

        repeatCoroutine = StartCoroutine(RepeatAction());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopRepeat();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopRepeat();
    }

    private IEnumerator RepeatAction()
    {
        onClick?.Invoke();

        yield return new WaitForSeconds(repeatDelay);

        while (isPointerDown)
        {
            onClick?.Invoke();
            hasRepeated = true;

            yield return new WaitForSeconds(repeatInterval);
        }
    }

    private void StopRepeat()
    {
        isPointerDown = false;

        if (repeatCoroutine != null)
        {
            StopCoroutine(repeatCoroutine);
            repeatCoroutine = null;
        }
    }
}