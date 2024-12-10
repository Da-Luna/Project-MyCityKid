using UnityEngine;

public class UIQuitGame : MonoBehaviour
{
    public void ButtonQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("Quit Game Button was pressed");
#endif
    }
}
