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
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, bottomOffset);
        rect.sizeDelta = new Vector2(width, 60f);

        label = gameObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.text = string.Empty;
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnItemPickedUp += Show;
        }
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnItemPickedUp -= Show;
        }
    }

    private void Show(string text)
    {
        label.text = text;

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        label.text = string.Empty;
    }
}