using UnityEngine;
using TMPro;

public class TextColorPulseAlpha : MonoBehaviour
{
    [SerializeField]
    float minAlpha = 0f; // The minimum alpha value

    [SerializeField]
    float maxAlpha = 1f; // The maximum alpha value

    [SerializeField]
    float m_SpeedMultiplier = 1f;  // You can adjust this value from the Inspector

    protected TMP_Text m_Text;
    protected Color m_Color;

    protected bool fadingOut = true; // To determine if we're fading out or in

    void Start()
    {
        if (m_Text == null)
        {
            m_Text = GetComponent<TMP_Text>();
            m_Color = m_Text.color; // Cache the initial color
        }
    }

    void OnDisable()
    {
        if (m_Text != null)
            m_Color.a = maxAlpha;
    }

    void Update()
    {
        if (fadingOut)
        {
            // Fade out by reducing alpha value
            m_Color.a = Mathf.MoveTowards(m_Color.a, minAlpha, m_SpeedMultiplier * Time.deltaTime);
            if (m_Color.a <= minAlpha)
            {
                fadingOut = false; // Start fading in once we've reached the minimum alpha
            }
        }
        else
        {
            // Fade in by increasing alpha value
            m_Color.a = Mathf.MoveTowards(m_Color.a, maxAlpha, m_SpeedMultiplier * Time.deltaTime);
            if (m_Color.a >= maxAlpha)
            {
                fadingOut = true; // Start fading out once we've reached the maximum alpha
            }
        }

        m_Text.color = m_Color; // Apply the updated color to the text
    }
}
