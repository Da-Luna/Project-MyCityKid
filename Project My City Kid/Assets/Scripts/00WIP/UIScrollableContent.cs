using System.Collections;
using UnityEngine;
using TMPro;

public class UIScrollableContent : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField, Tooltip("Text component displaying the current page number.")]
    TextMeshProUGUI currentPageNumberText;
    [SerializeField, Tooltip("Text component displaying the total number of pages.")]
    TextMeshProUGUI totalPagesText;

    [Header("Transition Settings")]
    [SerializeField, Tooltip("Time in seconds for transition buffer.")]
    float timeBeforeSetActive = 0.85f;
    WaitForSeconds timeBeforeSet;

    [Header("Content Array")]
    [SerializeField, Tooltip("Array of GameObjects representing content entries.")]
    GameObject[] contentEntry;

    int tabsEntryIndex; // Index of the currently active content entry
    bool isSliding; // Flag indicating whether a transition animation is currently playing

    // All UI Animator Controller use the same animation names
    const string uILAnimIn = "UILIn";
    const string uILAnimOut = "UILOut";
    const string uIRAnimIn = "UIRIn";
    const string uIRAnimOut = "UIROut";


    #region Initialitation and OnEnable Method
    private void InitEntries()
    {
        int totalPages = 0;
        foreach (GameObject entry in contentEntry)
        {
            entry.SetActive(false);
            totalPages++;
        }

        totalPagesText.text = totalPages.ToString();

        tabsEntryIndex = 0;
        contentEntry[tabsEntryIndex].SetActive(true);
    }
    private void OnEnable()
    {
        InitEntries();
        timeBeforeSet = new WaitForSeconds(timeBeforeSetActive);
    }
    #endregion // Initialitation and OnEnable Method

    #region Button OnClick Events
    public void ButtonNextEntry()
    {
        if (isSliding) return;

        GameObject tmpObjOut = contentEntry[tabsEntryIndex];
        StartCoroutine(SetActiveFalseDelay(tmpObjOut));

        Animator tmpAnimOut = tmpObjOut.GetComponent<Animator>();
        PlayAnimation(tmpAnimOut, uILAnimOut);

        tabsEntryIndex++;
        if (tabsEntryIndex >= contentEntry.Length)
            tabsEntryIndex = 0;

        GameObject tmpObjIn = contentEntry[tabsEntryIndex];
        tmpObjIn.SetActive(true);

        Animator tmpAnimIn = tmpObjIn.GetComponent<Animator>();
        PlayAnimation(tmpAnimIn, uIRAnimIn);

        SetCurrentPageNumber();
    }
    public void ButtonPreviewsEntry()
    {
        if (isSliding) return;

        GameObject tmpObjOut = contentEntry[tabsEntryIndex];
        StartCoroutine(SetActiveFalseDelay(tmpObjOut));

        Animator tmpAnimOut = tmpObjOut.GetComponent<Animator>();
        PlayAnimation(tmpAnimOut, uIRAnimOut);

        tabsEntryIndex--;
        if (tabsEntryIndex < 0)
            tabsEntryIndex = contentEntry.Length - 1;

        GameObject tmpObjIn = contentEntry[tabsEntryIndex];
        tmpObjIn.SetActive(true);

        Animator tmpAnimIn = tmpObjIn.GetComponent<Animator>();
        PlayAnimation(tmpAnimIn, uILAnimIn);

        SetCurrentPageNumber();
    }
    #endregion // Button OnClick Events

    #region Handles the Scrollable Content
    private void SetCurrentPageNumber()
    {
        int currentPageNumber = tabsEntryIndex + 1;
        currentPageNumberText.text = currentPageNumber.ToString();
    }

    /// <summary>
    /// Plays the specified animation on the given animator.
    /// </summary>
    /// <param name="animator">The animator component.</param>
    /// <param name="animName">The name of the animation to play.</param>
    private void PlayAnimation(Animator animator, string animName)
    {
        animator.Play(animName);
    }

    /// <summary>
    /// Delays setting an object inactive to allow for transition buffer.
    /// </summary>
    /// <param name="obj">The object to set inactive.</param>
    /// <returns></returns>
    private IEnumerator SetActiveFalseDelay(GameObject obj)
    {
        isSliding = true;

        yield return timeBeforeSet;
        obj.SetActive(false);
        isSliding = false;
    }
    #endregion // Handles the Scrollable Content
}
