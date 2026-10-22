using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Level Scene Names")]
    [SerializeField] private string level1SceneName = "PrototypeScene";
    [SerializeField] private string level2SceneName = "Level2";
    [SerializeField] private string level3SceneName = "Level3";
    [SerializeField] private string tutorialSceneName = "TutorialScene";

    void Awake()
    {
        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        SetPanelActive(mainPanel);
    }

    public void ShowPlayPanel()
    {
        SetPanelActive(playPanel);
    }

    public void ShowLevelSelectPanel()
    {
        SetPanelActive(levelSelectPanel);
    }

    public void ShowSettingsPanel()
    {
        SetPanelActive(settingsPanel);
    }

    private void SetPanelActive(GameObject targetPanel)
    {
        if (mainPanel != null) mainPanel.SetActive(mainPanel == targetPanel);
        if (playPanel != null) playPanel.SetActive(playPanel == targetPanel);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(levelSelectPanel == targetPanel);
        if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == targetPanel);
    }

    public void LoadLevel1() => LoadScene(level1SceneName);
    public void LoadLevel2() => LoadScene(level2SceneName);
    public void LoadLevel3() => LoadScene(level3SceneName);
    public void LoadTutorial() => LoadScene(tutorialSceneName);

    private void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}