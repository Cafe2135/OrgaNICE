using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;

    [Header("Theme Settings")]
    [SerializeField] private Sprite menuBackgroundSprite;
    [SerializeField] private Sprite buttonSprite;

    private GameObject pausePanel;

    void Awake()
    {
        BuildPauseMenuUI();
        SetPauseMenuVisible(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!LevelEvaluationUI.IsEvaluating && !StorageInteractionController.IsInStorageMode && !ChecklistUI.IsChecklistOpen)
            {
                if (IsPaused) Resume();
                else Pause();
            }
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        SetPauseMenuVisible(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SetPauseMenuVisible(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(0); // Index 0 or your Main Menu scene name
    }

    private void SetPauseMenuVisible(bool visible)
    {
        if (pausePanel != null) pausePanel.SetActive(visible);
    }

    private void BuildPauseMenuUI()
    {
        Transform existing = transform.Find("ThemedPausePanel");
        if (existing != null) DestroyImmediate(existing.gameObject);

        pausePanel = new GameObject("ThemedPausePanel", typeof(RectTransform), typeof(Image));
        pausePanel.transform.SetParent(transform, false);

        RectTransform panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(320f, 320f);

        Image img = pausePanel.GetComponent<Image>();
        if (menuBackgroundSprite != null)
        {
            img.sprite = menuBackgroundSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }
        else
        {
            img.sprite = null;
            img.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);
        }

        CreateTitle("PAUSED", pausePanel.transform, new Vector2(0f, 110f));

        CreateButton("ResumeBtn", pausePanel.transform, "RESUME", new Vector2(0f, 40f), Resume);
        CreateButton("RestartBtn", pausePanel.transform, "RESTART", new Vector2(0f, -20f), RestartLevel);
        CreateButton("MenuBtn", pausePanel.transform, "MAIN MENU", new Vector2(0f, -80f), LoadMainMenu);
    }

    private void CreateTitle(string text, Transform parent, Vector2 pos)
    {
        GameObject go = new GameObject("TitleText", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(280f, 40f);

        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.yellow;
        tmp.raycastTarget = false;
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
        rect.sizeDelta = new Vector2(220f, 44f);

        Image img = go.GetComponent<Image>();
        if (buttonSprite != null)
        {
            img.sprite = buttonSprite;
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
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }
}