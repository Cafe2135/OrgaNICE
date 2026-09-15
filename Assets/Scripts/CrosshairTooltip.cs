using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrosshairTooltip : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float rayDistance = 3.5f;
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightTint = new Color(1.25f, 1.25f, 1.25f, 1f);

    private Camera playerCamera;
    private PlayerInteraction playerInteraction;
    private GameObject tooltipPanel;
    private TMP_Text tooltipText;

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
            // Ignore standalone items (handled by Subtitle UI)
            if (hit.collider.GetComponent<InteractableItem>() != null)
            {
                ClearHighlight();
                HideTooltip();
                return;
            }

            // 1. Check Drawer / Small Storage
            SmallStorage storage = hit.collider.GetComponentInParent<SmallStorage>();
            if (storage != null)
            {
                ShowTooltip("[F] Open Storage Drawer");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }

            // 2. Check Trash Bin
            TrashBin bin = hit.collider.GetComponentInParent<TrashBin>();
            if (bin != null)
            {
                ShowTooltip("Trash Bin");
                ApplyHighlight(hit.collider.GetComponent<Renderer>());
                return;
            }

            // 3. Check Placement Surface
            PlacementZone zone = hit.collider.GetComponentInParent<PlacementZone>();
            if (zone != null)
            {
                ShowTooltip("Placement Area");
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
        if (tooltipText != null) tooltipText.text = text;
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void BuildUI()
    {
        tooltipPanel = new GameObject("TooltipPanel", typeof(RectTransform), typeof(Image));
        tooltipPanel.transform.SetParent(transform, false);

        var panelRect = tooltipPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -60f);
        panelRect.sizeDelta = new Vector2(300f, 36f);

        var img = tooltipPanel.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);
        img.raycastTarget = false;

        var textGo = new GameObject("TooltipText", typeof(RectTransform));
        textGo.transform.SetParent(tooltipPanel.transform, false);

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        tooltipText = textGo.AddComponent<TextMeshProUGUI>();
        tooltipText.fontSize = 17;
        tooltipText.alignment = TextAlignmentOptions.Center;
        tooltipText.color = Color.white;
        tooltipText.raycastTarget = false;
    }

    private void OnDisable()
    {
        ClearHighlight();
        HideTooltip();
    }
}