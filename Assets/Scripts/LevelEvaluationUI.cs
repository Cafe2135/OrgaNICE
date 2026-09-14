using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LevelEvaluationUI : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    public static bool IsEvaluating { get; private set; } = false;

    private GameObject panel;
    private TMP_Text scoreText;
    private TMP_Text starsText;

    void Awake()
    {
        EnsureEventSystem();
        BuildUI();
        panel.SetActive(false);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystemGo);
        }
    }

    public void ShowEvaluation(int score, int stars)
    {
        IsEvaluating = true;
        panel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (scoreText != null)
        {
            scoreText.text = $"Final Score: {score}";
        }

        if (starsText != null)
        {
            string starRating = "";
            for (int i = 0; i < 3; i++)
            {
                starRating += (i < stars) ? "[ * ] " : "[   ] ";
            }
            starsText.text = starRating.TrimEnd();
        }
    }

    private void RestartLevel()
    {
        IsEvaluating = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ReturnToMainMenu()
    {
        IsEvaluating = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void BuildUI()
    {
        panel = new GameObject("EvaluationPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var img = panel.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.85f);
        img.raycastTarget = true;

        CreateLabel(panel.transform, "LEVEL COMPLETE", new Vector2(0f, 150f), 40, Color.yellow);
        
        starsText = CreateLabel(panel.transform, "[   ] [   ] [   ]", new Vector2(0f, 70f), 48, new Color(1f, 0.84f, 0f));
        scoreText = CreateLabel(panel.transform, "Final Score: 0", new Vector2(0f, 0f), 30, Color.white);

        CreateButton(panel.transform, "Retry Level", new Vector2(-120f, -100f), RestartLevel);
        CreateButton(panel.transform, "Main Menu", new Vector2(120f, -100f), ReturnToMainMenu);
    }

    private TMP_Text CreateLabel(Transform parent, string text, Vector2 anchoredPosition, int fontSize, Color color)
    {
        var go = new GameObject("Label_" + text, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(500f, 80f);

        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private void CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(200f, 50f);

        var image = go.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);
        image.raycastTarget = true;

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        tmp.raycastTarget = false;
    }
}