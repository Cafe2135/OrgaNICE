using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChecklistUI : MonoBehaviour
{
    public static bool IsChecklistOpen { get; private set; } = false;

    [Header("Theme Sprites")]
    [SerializeField] private Sprite slicedPanelSprite;
    [SerializeField] private Sprite customStarSprite;

    [Header("Panel Dimensions")]
    [SerializeField] private Vector2 panelSize = new Vector2(520f, 420f);
    [SerializeField] private float textLeftMargin = 40f;
    [SerializeField] private float textRightMargin = 40f;

    [Header("Vertical Line Positions (Y Offsets)")]
    [SerializeField] private float titleY = 150f;
    [SerializeField] private float minorHeaderY = 105f;
    [SerializeField] private float minorTrashY = 70f;
    [SerializeField] private float minorNeededY = 35f;
    [SerializeField] private float majorHeaderY = -10f;
    [SerializeField] private float star1Y = -45f;
    [SerializeField] private float star2Y = -80f;
    [SerializeField] private float star3Y = -115f;

    [Header("Font Sizes")]
    [SerializeField] private int titleFontSize = 22;
    [SerializeField] private int sectionHeaderFontSize = 16;
    [SerializeField] private int itemFontSize = 16;

    private GameObject checklistPanel;
    private TMP_Text minorTrashText;
    private TMP_Text minorNeededText;
    private TMP_Text star1Text;
    private TMP_Text star2Text;
    private TMP_Text star3Text;

    private GameObject hudStarContainer;
    private Image star1Icon;
    private Image star2Icon;
    private Image star3Icon;

    private bool isSubscribed = false;

    private void Awake()
    {
        BuildHUDStarDisplay();
        BuildChecklistOverlay();
        SetChecklistVisible(false);
    }

    private void Start()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        
        if (IsChecklistOpen)
        {
            Time.timeScale = 1f;
            IsChecklistOpen = false;
        }
    }

    private void Update()
    {
        if (!isSubscribed) TrySubscribe();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!PauseMenu.IsPaused && !LevelEvaluationUI.IsEvaluating && !StorageInteractionController.IsInStorageMode)
            {
                ToggleChecklist();
            }
        }
    }

    private void TrySubscribe()
    {
        if (isSubscribed) return;

        if (LevelObjectiveManager.Instance != null)
        {
            LevelObjectiveManager.Instance.OnObjectivesUpdated += RefreshUI;
            LevelObjectiveManager.Instance.OnStarEarned += HandleStarEarned;
            isSubscribed = true;
            RefreshUI();
        }
    }

    private void Unsubscribe()
    {
        if (!isSubscribed) return;

        if (LevelObjectiveManager.Instance != null)
        {
            LevelObjectiveManager.Instance.OnObjectivesUpdated -= RefreshUI;
            LevelObjectiveManager.Instance.OnStarEarned -= HandleStarEarned;
        }
        isSubscribed = false;
    }

    private void ToggleChecklist()
    {
        IsChecklistOpen = !IsChecklistOpen;
        SetChecklistVisible(IsChecklistOpen);

        if (IsChecklistOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RefreshUI()
    {
        var mgr = LevelObjectiveManager.Instance;
        if (mgr == null) return;

        if (minorTrashText != null) minorTrashText.text = $"[ {(mgr.MinorTrashComplete ? "✓" : " ")} ] Throw out the trash ({mgr.GetTrashProgressText()})";
        if (minorNeededText != null) minorNeededText.text = $"[ {(mgr.MinorNeededItemsComplete ? "✓" : " ")} ] Find needed items ({mgr.GetNeededItemsProgressText()})";

        if (star1Text != null) star1Text.text = $"[ {(mgr.Star1Earned ? "✓" : " ")} ] ⭐ Star 1: Complete all minor objectives";
        if (star2Text != null) star2Text.text = $"[ {(mgr.Star2Earned ? "✓" : " ")} ] ⭐ Star 2: Place items in valid zones ({mgr.GetPlacementProgressText()})";
        if (star3Text != null) star3Text.text = $"[ {(mgr.Star3Earned ? "✓" : " ")} ] ⭐ Star 3: Reach target score ({mgr.GetScoreProgressText()})";

        int totalStarsEarned = 0;
        if (mgr.Star1Earned) totalStarsEarned++;
        if (mgr.Star2Earned) totalStarsEarned++;
        if (mgr.Star3Earned) totalStarsEarned++;

        Color activeColor = Color.white;
        Color dimColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);

        if (star1Icon != null) star1Icon.color = (totalStarsEarned >= 1) ? activeColor : dimColor;
        if (star2Icon != null) star2Icon.color = (totalStarsEarned >= 2) ? activeColor : dimColor;
        if (star3Icon != null) star3Icon.color = (totalStarsEarned >= 3) ? activeColor : dimColor;
    }

    private void HandleStarEarned(int starIndex)
    {
        RefreshUI();
    }

    private void SetChecklistVisible(bool visible)
    {
        if (checklistPanel != null)
        {
            checklistPanel.SetActive(visible);
            if (visible) RefreshUI();
        }
    }

    private void BuildHUDStarDisplay()
    {
        Transform existing = transform.Find("ThemedHUDStarContainer");
        if (existing != null) DestroyImmediate(existing.gameObject);

        hudStarContainer = new GameObject("ThemedHUDStarContainer", typeof(RectTransform));
        hudStarContainer.transform.SetParent(transform, false);

        RectTransform rect = hudStarContainer.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -20f);
        rect.sizeDelta = new Vector2(160f, 50f);

        star1Icon = CreateHUDStar("Star1", hudStarContainer.transform, new Vector2(0f, 0f));
        star2Icon = CreateHUDStar("Star2", hudStarContainer.transform, new Vector2(50f, 0f));
        star3Icon = CreateHUDStar("Star3", hudStarContainer.transform, new Vector2(100f, 0f));
    }

    private Image CreateHUDStar(string name, Transform parent, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(42f, 42f);

        Image img = go.GetComponent<Image>();
        if (customStarSprite != null)
        {
            img.sprite = customStarSprite;
        }
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
        return img;
    }

    private void BuildChecklistOverlay()
    {
        Transform existing = transform.Find("ThemedChecklistPanel");
        if (existing != null) DestroyImmediate(existing.gameObject);

        checklistPanel = new GameObject("ThemedChecklistPanel", typeof(RectTransform), typeof(Image));
        checklistPanel.transform.SetParent(transform, false);

        RectTransform panelRect = checklistPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = panelSize;

        Image img = checklistPanel.GetComponent<Image>();
        if (slicedPanelSprite != null)
        {
            img.sprite = slicedPanelSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.sprite = null;
            img.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        }

        // Title & Section Headers (Centered)
        CreateFullWidthText("Header", checklistPanel.transform, "ROOM CHECKLIST (TAB)", titleFontSize, titleY, textLeftMargin, textRightMargin, TextAlignmentOptions.Center, Color.yellow);
        CreateFullWidthText("MinorHeader", checklistPanel.transform, "--- MINOR OBJECTIVES ---", sectionHeaderFontSize, minorHeaderY, textLeftMargin, textRightMargin, TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f));
        CreateFullWidthText("MajorHeader", checklistPanel.transform, "--- MAJOR OBJECTIVES (STARS) ---", sectionHeaderFontSize, majorHeaderY, textLeftMargin, textRightMargin, TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f));

        // Objective Items (Left-Aligned across full interior width)
        minorTrashText = CreateFullWidthText("MinorTrash", checklistPanel.transform, "", itemFontSize, minorTrashY, textLeftMargin, textRightMargin, TextAlignmentOptions.Left, Color.white);
        minorNeededText = CreateFullWidthText("MinorNeeded", checklistPanel.transform, "", itemFontSize, minorNeededY, textLeftMargin, textRightMargin, TextAlignmentOptions.Left, Color.white);

        star1Text = CreateFullWidthText("Star1Text", checklistPanel.transform, "", itemFontSize, star1Y, textLeftMargin, textRightMargin, TextAlignmentOptions.Left, Color.white);
        star2Text = CreateFullWidthText("Star2Text", checklistPanel.transform, "", itemFontSize, star2Y, textLeftMargin, textRightMargin, TextAlignmentOptions.Left, Color.white);
        star3Text = CreateFullWidthText("Star3Text", checklistPanel.transform, "", itemFontSize, star3Y, textLeftMargin, textRightMargin, TextAlignmentOptions.Left, Color.white);
    }

    private TMP_Text CreateFullWidthText(string name, Transform parent, string defaultContent, int fontSize, float yPos, float leftMargin, float rightMargin, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        // Stretch horizontally across parent card while anchoring around Y position
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(leftMargin, yPos - 18f);
        rect.offsetMax = new Vector2(-rightMargin, yPos + 18f);

        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultContent;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }
}