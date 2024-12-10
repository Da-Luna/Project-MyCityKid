using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UISceneTransitionTime : MonoBehaviour
{
    [SerializeField]
    float timeBeforeChangeScene = 1.0f;

    int indexNo;
    bool isStarted;

    public void ButtonSceneTransition(int sIndex)
    {
        if (isStarted) return;

        indexNo = sIndex;
        isStarted = true;

#if UNITY_EDITOR
        string buttonName = gameObject.name;
        Debug.Log($"GameObject {buttonName} was clicked. Start loading scene with index number: {sIndex}");
#endif
        StartCoroutine(TimeBeforeChangeScene());
    }
    IEnumerator TimeBeforeChangeScene()
    {
        yield return new WaitForSeconds(timeBeforeChangeScene);

        SceneManager.LoadScene(indexNo);
#if UNITY_EDITOR
        string buttonName = gameObject.name;
        Debug.Log($"GameObject {buttonName} was clicked. Loading scene with index number: {indexNo}");
#endif
    }
}
