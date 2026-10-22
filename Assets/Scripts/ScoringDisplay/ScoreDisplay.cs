using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI Theme Settings")]
    [SerializeField] private Sprite slicedPanelSprite;
    [SerializeField] private Vector4 padding = new Vector4(20, 20, 10, 10);

    private TMP_Text scoreText;
    private int currentScore = 0;

    public int CurrentScore => currentScore;

    void Awake()
    {
        BuildThemedScorePanel();
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateDisplay();
    }

    public void SetScore(int points)
    {
        currentScore = points;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {currentScore:D5}";
        }
    }

    private void BuildThemedScorePanel()
    {
        Transform existing = transform.Find("ThemedScorePanel");
        if (existing != null) DestroyImmediate(existing.gameObject);

        GameObject panel = new GameObject("ThemedScorePanel", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        panel.transform.SetParent(transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);

        Image img = panel.GetComponent<Image>();
        img.raycastTarget = false;

        if (slicedPanelSprite != null)
        {
            img.sprite = slicedPanelSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.sprite = null;
            img.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
        }

        HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset((int)padding.x, (int)padding.y, (int)padding.z, (int)padding.w);
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject textGo = new GameObject("ScoreText", typeof(RectTransform));
        textGo.transform.SetParent(panel.transform, false);

        scoreText = textGo.AddComponent<TextMeshProUGUI>();
        scoreText.fontSize = 22;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.color = Color.white;
        scoreText.raycastTarget = false;

        UpdateDisplay();
    }
}