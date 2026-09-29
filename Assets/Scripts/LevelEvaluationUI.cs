using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelEvaluationUI : MonoBehaviour
{
    public static LevelEvaluationUI Instance { get; private set; }
    public static bool IsEvaluating { get; private set; } = false;

    [Header("Theme Settings")]
    [SerializeField] private Sprite slicedPanelSprite;
    [SerializeField] private Sprite customStarSprite;

    private GameObject evalPanel;
    private TMP_Text scoreText;
    private TMP_Text summaryText;
    private Image star1Icon;
    private Image star2Icon;
    private Image star3Icon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildEvaluationUI();
        SetEvaluationVisible(false);
    }

    public void ShowEvaluation()
    {
        IsEvaluating = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PopulateResults();
        SetEvaluationVisible(true);
    }

    public void PopulateResults()
    {
        var mgr = LevelObjectiveManager.Instance;
        if (mgr == null) return;

        // Force objective evaluation on exit
        mgr.EvaluateObjectives();

        int totalStars = 0;
        if (mgr.Star1Earned) totalStars++;
        if (mgr.Star2Earned) totalStars++;
        if (mgr.Star3Earned) totalStars++;

        Color activeColor = Color.white;
        Color dimColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);

        if (star1Icon != null) star1Icon.color = (totalStars >= 1) ? activeColor : dimColor;
        if (star2Icon != null) star2Icon.color = (totalStars >= 2) ? activeColor : dimColor;
        if (star3Icon != null) star3Icon.color = (totalStars >= 3) ? activeColor : dimColor;

        ScoreDisplay scoreDisplay = FindFirstObjectByType<ScoreDisplay>();
        int finalScore = scoreDisplay != null ? scoreDisplay.CurrentScore : 0;

        if (scoreText != null)
        {
            scoreText.text = $"FINAL SCORE: {finalScore}";
        }

        if (summaryText != null)
        {
            summaryText.text = $"STARS EARNED: {totalStars} / 3\n\n" +
                               $"Trash Objective: {(mgr.MinorTrashComplete ? "✓ Complete" : "✗ Incomplete")}\n" +
                               $"Needed Items Objective: {(mgr.MinorNeededItemsComplete ? "✓ Complete" : "✗ Incomplete")}\n" +
                               $"Placement Objective: {mgr.GetPlacementProgressText()}";
        }
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        IsEvaluating = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsEvaluating = false;
        SceneManager.LoadScene(0);
    }

    private void SetEvaluationVisible(bool visible)
    {
        if (evalPanel != null) evalPanel.SetActive(visible);
    }

    private void BuildEvaluationUI()
    {
        Transform existing = transform.Find("ThemedEvaluationPanel");
        if (existing != null) DestroyImmediate(existing.gameObject);

        evalPanel = new GameObject("ThemedEvaluationPanel", typeof(RectTransform), typeof(Image));
        evalPanel.transform.SetParent(transform, false);

        RectTransform panelRect = evalPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(480f, 420f);

        Image img = evalPanel.GetComponent<Image>();
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

        // Header Title
        CreateText("Header", evalPanel.transform, "LEVEL EVALUATION", 24, new Vector2(0f, 160f), TextAlignmentOptions.Center, Color.yellow);

        // Star Icons Display
        GameObject starContainer = new GameObject("StarsContainer", typeof(RectTransform));
        starContainer.transform.SetParent(evalPanel.transform, false);
        RectTransform starRect = starContainer.GetComponent<RectTransform>();
        starRect.anchorMin = new Vector2(0.5f, 0.5f);
        starRect.anchorMax = new Vector2(0.5f, 0.5f);
        starRect.pivot = new Vector2(0.5f, 0.5f);
        starRect.anchoredPosition = new Vector2(0f, 95f);
        starRect.sizeDelta = new Vector2(180f, 50f);

        star1Icon = CreateStarIcon("Star1", starContainer.transform, new Vector2(-60f, 0f));
        star2Icon = CreateStarIcon("Star2", starContainer.transform, new Vector2(0f, 0f));
        star3Icon = CreateStarIcon("Star3", starContainer.transform, new Vector2(60f, 0f));

        // Score Text
        scoreText = CreateText("ScoreText", evalPanel.transform, "FINAL SCORE: 00000", 20, new Vector2(0f, 35f), TextAlignmentOptions.Center, Color.white);

        // Summary Breakdown
        summaryText = CreateText("SummaryText", evalPanel.transform, "", 15, new Vector2(0f, -30f), TextAlignmentOptions.Center, new Color(0.85f, 0.85f, 0.85f));

        // Buttons
        CreateButton("RetryBtn", evalPanel.transform, "RETRY", new Vector2(-90f, -145f), RetryLevel);
        CreateButton("MenuBtn", evalPanel.transform, "MAIN MENU", new Vector2(90f, -145f), LoadMainMenu);
    }

    private Image CreateStarIcon(string name, Transform parent, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(48f, 48f);

        Image img = go.GetComponent<Image>();
        if (customStarSprite != null) img.sprite = customStarSprite;
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.4f);
        return img;
    }

    private TMP_Text CreateText(string name, Transform parent, string content, int fontSize, Vector2 anchoredPos, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(420f, 110f);

        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    private void CreateButton(string name, Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(150f, 44f);

        Image img = go.GetComponent<Image>();
        if (slicedPanelSprite != null)
        {
            img.sprite = slicedPanelSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.sprite = null;
            img.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
        }

        Button btn = go.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        GameObject textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }
}