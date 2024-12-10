using UnityEngine;

public class CanvasRotToCamera : MonoBehaviour
{
    private RectTransform m_RectTransform;
    private Camera mainCamera;

    void OnEnable()
    {
        m_RectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 directionToCamera = m_RectTransform.position - mainCamera.transform.position;
        m_RectTransform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
    }
}
