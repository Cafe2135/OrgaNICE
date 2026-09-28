using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChecklistUI : MonoBehaviour
{
    public static bool IsChecklistOpen { get; private set; } = false;

    private GameObject checklistPanel;
    private TMP_Text minorTrashText;
    private TMP_Text minorNeededText;
    private TMP_Text star1Text;
    private TMP_Text star2Text;
    private TMP_Text star3Text;

    // Top-Left HUD Stars (Progressive Fill: Left to Right)
    private GameObject hudStarContainer;
    private Image star1Icon; // Leftmost star
    private Image star2Icon; // Middle star
    private Image star3Icon; // Rightmost star

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
        // Guarantee subscription if LevelObjectiveManager initialized late
        if (!isSubscribed)
        {
            TrySubscribe();
        }

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

        // Minor Objectives
        if (minorTrashText != null)
            minorTrashText.text = $"[ {(mgr.MinorTrashComplete ? "✓" : " ")} ] Throw out the trash ({mgr.GetTrashProgressText()})";
        if (minorNeededText != null)
            minorNeededText.text = $"[ {(mgr.MinorNeededItemsComplete ? "✓" : " ")} ] Find needed items ({mgr.GetNeededItemsProgressText()})";

        // Major Objectives / Stars Text
        if (star1Text != null)
            star1Text.text = $"[ {(mgr.Star1Earned ? "✓" : " ")} ] ⭐ Star 1: Complete all minor objectives";
        if (star2Text != null)
            star2Text.text = $"[ {(mgr.Star2Earned ? "✓" : " ")} ] ⭐ Star 2: Place items in valid zones ({mgr.GetPlacementProgressText()})";
        if (star3Text != null)
            star3Text.text = $"[ {(mgr.Star3Earned ? "✓" : " ")} ] ⭐ Star 3: Reach target score ({mgr.GetScoreProgressText()})";

        // Count total stars earned (progressive left-to-right fill)
        int totalStarsEarned = 0;
        if (mgr.Star1Earned) totalStarsEarned++;
        if (mgr.Star2Earned) totalStarsEarned++;
        if (mgr.Star3Earned) totalStarsEarned++;

        Color yellow = Color.yellow;
        Color dim = new Color(0.3f, 0.3f, 0.3f, 0.6f);

        if (star1Icon != null) star1Icon.color = (totalStarsEarned >= 1) ? yellow : dim;
        if (star2Icon != null) star2Icon.color = (totalStarsEarned >= 2) ? yellow : dim;
        if (star3Icon != null) star3Icon.color = (totalStarsEarned >= 3) ? yellow : dim;
    }

    private void HandleStarEarned(int starIndex)
    {
        RefreshUI();
        Debug.Log($"⭐ Star {starIndex} Unlocked!");
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
        hudStarContainer = new GameObject("HUDStarContainer", typeof(RectTransform));
        hudStarContainer.transform.SetParent(transform, false);

        var rect = hudStarContainer.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(20f, -20f);
        rect.sizeDelta = new Vector2(150f, 45f);

        star1Icon = CreateHUDStar("Star1", hudStarContainer.transform, new Vector2(0f, 0f));
        star2Icon = CreateHUDStar("Star2", hudStarContainer.transform, new Vector2(50f, 0f));
        star3Icon = CreateHUDStar("Star3", hudStarContainer.transform, new Vector2(100f, 0f));
    }

    private Image CreateHUDStar(string name, Transform parent, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(40f, 40f);

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        return img;
    }

    private void BuildChecklistOverlay()
    {
        checklistPanel = new GameObject("ChecklistPanel", typeof(RectTransform), typeof(Image));
        checklistPanel.transform.SetParent(transform, false);

        var panelRect = checklistPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(480f, 380f);

        var img = checklistPanel.GetComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

        CreateText("Header", checklistPanel.transform, "ROOM CHECKLIST (TAB)", 22, new Vector2(0f, 150f), TextAlignmentOptions.Center, Color.yellow);

        CreateText("MinorHeader", checklistPanel.transform, "--- MINOR OBJECTIVES ---", 16, new Vector2(0f, 105f), TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f));
        minorTrashText = CreateText("MinorTrash", checklistPanel.transform, "", 16, new Vector2(-190f, 70f), TextAlignmentOptions.Left, Color.white);
        minorNeededText = CreateText("MinorNeeded", checklistPanel.transform, "", 16, new Vector2(-190f, 35f), TextAlignmentOptions.Left, Color.white);

        CreateText("MajorHeader", checklistPanel.transform, "--- MAJOR OBJECTIVES (STARS) ---", 16, new Vector2(0f, -10f), TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f));
        star1Text = CreateText("Star1Text", checklistPanel.transform, "", 16, new Vector2(-190f, -45f), TextAlignmentOptions.Left, Color.white);
        star2Text = CreateText("Star2Text", checklistPanel.transform, "", 16, new Vector2(-190f, -80f), TextAlignmentOptions.Left, Color.white);
        star3Text = CreateText("Star3Text", checklistPanel.transform, "", 16, new Vector2(-190f, -115f), TextAlignmentOptions.Left, Color.white);
    }

    private TMP_Text CreateText(string name, Transform parent, string defaultContent, int fontSize, Vector2 anchoredPos, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(420f, 30f);

        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultContent;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }
}