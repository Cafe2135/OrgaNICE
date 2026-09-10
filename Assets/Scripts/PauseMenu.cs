using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    // Other scripts (PlayerController's look, etc.) can check this to freeze
    // themselves properly instead of relying only on Time.timeScale.
    public static bool IsPaused { get; private set; }

    private GameObject panel;

    void Awake()
    {
        BuildUI();
        panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RestartLevel()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BuildUI()
    {
        panel = new GameObject("Panel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        panel.transform.SetParent(transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0f, 0f, 0f, 0.75f);

        CreateLabel(panel.transform, "PAUSED", new Vector2(0f, 140f), 42);

        CreateButton(panel.transform, "Resume", new Vector2(0f, 40f), Resume);
        CreateButton(panel.transform, "Restart Level", new Vector2(0f, -20f), RestartLevel);
        CreateButton(panel.transform, "Exit", new Vector2(0f, -80f), ExitGame);
    }

    private void CreateLabel(Transform parent, string text, Vector2 anchoredPosition, int fontSize)
    {
        var go = new GameObject("Title", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(400f, 60f);

        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }

    private void CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(220f, 50f);

        var image = go.GetComponent<UnityEngine.UI.Image>();
        image.color = new Color(1f, 1f, 1f, 0.9f);

        var button = go.GetComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image; // needed for click/hover feedback since this is built in code, not the editor
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
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
    }
}