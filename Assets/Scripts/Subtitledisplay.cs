using System.Collections;
using TMPro;
using UnityEngine;

public class SubtitleDisplay : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float width = 500f;
    [SerializeField] private float bottomOffset = 110f;
    [SerializeField] private int fontSize = 28;

    private TMP_Text label;
    private Coroutine hideRoutine;

    void Awake()
    {
        var rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottomOffset);
            rect.sizeDelta = new Vector2(width, 60f);
        }

        // Use existing TMP_Text or create only if missing
        label = GetComponent<TMP_Text>();
        if (label == null)
        {
            label = gameObject.AddComponent<TextMeshProUGUI>();
        }

        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.text = string.Empty;
        label.raycastTarget = false;
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnItemPickedUp += ShowTimed;
        }
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnItemPickedUp -= ShowTimed;
        }
    }

    // Displays text for a temporary duration (e.g. quick pickup notification)
    public void ShowTimed(string text)
    {
        if (label == null) return;
        label.text = text;

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    // Displays persistent text while holding/inspecting an item
    public void ShowPersistent(string text)
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
        if (label != null)
        {
            label.text = text;
        }
    }

    public void Clear()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
        if (label != null)
        {
            label.text = string.Empty;
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (label != null)
        {
            label.text = string.Empty;
        }
    }
}