using UnityEngine;

public class UICanvasGroupeAlpha : MonoBehaviour
{
    [Range(0f, 1f)]
    public float startValue;
    [Range(0f, 1f)]
    public float targetValue;
    
    [Range(0.01f, 100f)]
    public float changeMultipier;

    CanvasGroup canvasGroup;

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
    }

    void Update()
    {
        if (canvasGroup.alpha != targetValue)
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetValue, changeMultipier * Time.deltaTime);
        else if (canvasGroup.alpha == targetValue)
            enabled = false;
#if UNITY_EDITOR
        else Debug.LogError("UICanvasGroupeAlpha: A problem occurred when changing the alpha value");
#endif
    }
}
