using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneTransition : MonoBehaviour
{
    public void ButtonSceneTransition(int sIndex)
    {
        SceneManager.LoadScene(sIndex);
#if UNITY_EDITOR
        string buttonName = gameObject.name;
        Debug.Log($"GameObject {buttonName} was clicked. Loading Scene with Index Number: {sIndex}");
#endif
    }
}
