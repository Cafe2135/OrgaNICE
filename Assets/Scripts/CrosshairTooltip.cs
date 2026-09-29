using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrosshairTooltip : MonoBehaviour
{
    [Header("UI Theme Settings")]
    [SerializeField] private Sprite slicedPanelSprite;
    [SerializeField] private Vector4 padding = new Vector4(20, 20, 10, 10);
    [SerializeField] private float verticalOffset = -70f;
    [SerializeField] private float maxTooltipWidth = 380f;

    [Header("Raycast Settings")]
    [SerializeField] private float rayDistance = 3.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightTint = new Color(1.25f, 1.25f, 1.25f, 1f);

    private Camera playerCamera;
    private PlayerInteraction playerInteraction;
    private GameObject tooltipPanel;
    private RectTransform panelRect;
    private Image panelImage;
    private TMP_Text tooltipText;
    private LayoutElement textLayoutElement;

    private Renderer currentRenderer;
    private Color originalColor;
    private bool hasOriginalColor = false;

    void Awake()
    {
        playerCamera = Camera.main;
        playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        BuildUI();
        HideTooltip();
    }

    void Update()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerInteraction == null) playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        if (PauseMenu.IsPaused || 
            LevelEvaluationUI.IsEvaluating || 
            StorageInteractionController.IsInStorageMode || 
            HeavyObjectController.IsDraggingObject ||
            ChecklistUI.IsChecklistOpen ||
            (playerInteraction != null && playerInteraction.IsHolding))
        {
            ClearHighlight();
            HideTooltip();
            return;
        }

        PerformHoverCheck();
    }

    private void PerformHoverCheck()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayers, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.GetComponent<InteractableItem>() != null)
            {
                ClearHighlight();
                HideTooltip();
                return;
            }

            // 1. Check Exit Door (LevelExit.cs)
            LevelExit door = hit.collider.GetComponentInParent<LevelExit>();
            if (door != null)
            {
                ShowTooltip("[E] Leave Room");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }

            // 2. Check Heavy Movable Props
            MovableObject movable = hit.collider.GetComponentInParent<MovableObject>();
            if (movable != null)
            {
                ShowTooltip($"[Hold Click] Push / Pull — {movable.ObjectName}");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }

            // 3. Check Storage Drawer
            SmallStorage storage = hit.collider.GetComponentInParent<SmallStorage>();
            if (storage != null)
            {
                ShowTooltip("[F] Open Storage Drawer");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }

            // 4. Check Trash Bin
            TrashBin bin = hit.collider.GetComponentInParent<TrashBin>();
            if (bin != null)
            {
                ShowTooltip("Trash Bin");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }

            // 5. Check Placement Area
            PlacementZone zone = hit.collider.GetComponentInParent<PlacementZone>();
            if (zone != null)
            {
                ShowTooltip("Placement Area");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }
            CabinetStorage cabinet = hit.collider.GetComponentInParent<CabinetStorage>();
            if (cabinet != null)
            {
                ShowTooltip("[F] Open Cabinet");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }
        }

        ClearHighlight();
        HideTooltip();
    }

    private void ApplyHighlight(Renderer newRenderer)
    {
        if (newRenderer == null) return;

        if (currentRenderer != newRenderer)
        {
            ClearHighlight();
            currentRenderer = newRenderer;

            if (currentRenderer.material.HasProperty("_Color"))
            {
                originalColor = currentRenderer.material.color;
                hasOriginalColor = true;
                currentRenderer.material.color = originalColor * highlightTint;
            }
        }
    }

    private void ClearHighlight()
    {
        if (currentRenderer != null && hasOriginalColor)
        {
            if (currentRenderer.material.HasProperty("_Color"))
            {
                currentRenderer.material.color = originalColor;
            }
        }

        currentRenderer = null;
        hasOriginalColor = false;
    }

    private void ShowTooltip(string text)
    {
        if (tooltipText == null || tooltipPanel == null) return;

        tooltipText.text = text;

        float preferredWidth = tooltipText.GetPreferredValues(text).x;
        textLayoutElement.enabled = preferredWidth > maxTooltipWidth;

        panelRect.anchoredPosition = new Vector2(0f, verticalOffset);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        tooltipPanel.SetActive(true);
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void BuildUI()
    {
        Transform existing = transform.Find("ThemedTooltipPanel");
        if (existing != null) DestroyImmediate(existing.gameObject);

        tooltipPanel = new GameObject("ThemedTooltipPanel", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        tooltipPanel.transform.SetParent(transform, false);

        panelRect = tooltipPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, verticalOffset);

        panelImage = tooltipPanel.GetComponent<Image>();
        panelImage.raycastTarget = false;

        if (slicedPanelSprite != null)
        {
            panelImage.sprite = slicedPanelSprite;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = Color.white;
        }
        else
        {
            panelImage.sprite = null;
            panelImage.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
        }

        HorizontalLayoutGroup layout = tooltipPanel.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset((int)padding.x, (int)padding.y, (int)padding.z, (int)padding.w);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = tooltipPanel.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject textGo = new GameObject("TooltipText", typeof(RectTransform), typeof(LayoutElement));
        textGo.transform.SetParent(tooltipPanel.transform, false);

        textLayoutElement = textGo.GetComponent<LayoutElement>();
        textLayoutElement.preferredWidth = maxTooltipWidth;
        textLayoutElement.enabled = false;

        tooltipText = textGo.AddComponent<TextMeshProUGUI>();
        tooltipText.fontSize = 17;
        tooltipText.alignment = TextAlignmentOptions.Center;
        tooltipText.color = Color.white;
        tooltipText.enableWordWrapping = true;
        tooltipText.raycastTarget = false;
    }

    private void OnDisable()
    {
        ClearHighlight();
        HideTooltip();
    }
}