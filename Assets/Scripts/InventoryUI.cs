using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventorySelector selector;
    [SerializeField] private int slotCount = 5;
    [SerializeField] private Vector2 slotSize = new Vector2(80f, 80f);
    [SerializeField] private float slotSpacing = 10f;
    [SerializeField] private float bottomOffset = 20f;
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color filledColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.2f, 0.9f);

    private UnityEngine.UI.Image[] slotBackgrounds;
    private UnityEngine.UI.Image[] slotIcons;

    void Awake()
    {
        SetupLayout();
        BuildSlots();
    }

    private void SetupLayout()
    {

        var rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);

        float totalWidth = slotCount * slotSize.x + Mathf.Max(0, slotCount - 1) * slotSpacing;
        rect.sizeDelta = new Vector2(totalWidth, slotSize.y);
        rect.anchoredPosition = new Vector2(0f, bottomOffset);

        var layout = GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        }
        layout.spacing = slotSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void BuildSlots()
    {
        slotBackgrounds = new UnityEngine.UI.Image[slotCount];
        slotIcons = new UnityEngine.UI.Image[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            var slot = new GameObject("Slot" + (i + 1), typeof(RectTransform), typeof(UnityEngine.UI.Image));
            slot.transform.SetParent(transform, false);
            slot.GetComponent<RectTransform>().sizeDelta = slotSize;

            var background = slot.GetComponent<UnityEngine.UI.Image>();
            background.color = emptyColor;
            slotBackgrounds[i] = background;

            var icon = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            icon.transform.SetParent(slot.transform, false);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var iconImage = icon.GetComponent<UnityEngine.UI.Image>();
            iconImage.enabled = false;
            slotIcons[i] = iconImage;
        }
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += Refresh;
        }
        if (selector != null)
        {
            selector.OnSelectionChanged += Refresh;
        }
        Refresh();
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= Refresh;
        }
        if (selector != null)
        {
            selector.OnSelectionChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        if (slotIcons == null || inventory == null) return;

        var entries = inventory.Items;
        int selectedIndex = selector != null ? selector.SelectedIndex : -1;

        for (int i = 0; i < slotIcons.Length; i++)
        {
            bool filled = i < entries.Count;
            slotIcons[i].sprite = filled ? entries[i].icon : null;
            slotIcons[i].enabled = filled;

            slotBackgrounds[i].color = i == selectedIndex ? selectedColor : (filled ? filledColor : emptyColor);
        }
    }
}