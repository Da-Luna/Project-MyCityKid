using UnityEngine;

public class UICanvasTrigGroupeAlpha : MonoBehaviour
{
    [Header("Canvas Reference")]
    [SerializeField]
    CanvasGroup canvasGroup;

    [Header("Setting")]
    [SerializeField, Range(0f, 1f)]
    float startValue;
    [SerializeField, Range(0f, 1f)]
    float targetValue;
    
    [SerializeField,Range(0.01f, 100f)]
    float changeMultipier;

    public bool StartFade { get; private set; }

    void OnEnable()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

#if UNITY_EDITOR
        if (canvasGroup != null)
            Debug.Log("UICanvasGroupeAlpha: canvasGroup is NULL, get component from GameObject");
        else if (canvasGroup == null)
        {
            Debug.LogError("UICanvasGroupeAlpha: canvasGroup still NULL after try to get component from GameObject");
            Debug.Break();
        }
#endif

        canvasGroup.alpha = startValue;
        StartFade = false;
    }

    public void ButtonStartFade()
    {
        StartFade = true;
    }

    void Update()
    {
        if (!StartFade) return;

        if (canvasGroup.alpha != targetValue)
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetValue, changeMultipier * Time.deltaTime);
        else if (canvasGroup.alpha == targetValue)
            enabled = false;
#if UNITY_EDITOR
        else Debug.LogError("UICanvasGroupeAlpha: A problem occurred when changing the alpha value");
#endif
    }
}
