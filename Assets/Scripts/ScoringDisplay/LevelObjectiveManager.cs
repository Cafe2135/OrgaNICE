using System;
using UnityEngine;

public class LevelObjectiveManager : MonoBehaviour
{
    public static LevelObjectiveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ScoreDisplay scoreDisplay;

    [Header("Level Configuration")]
    [SerializeField] private int totalTrashInLevel = 3;
    [SerializeField] private int totalNeededItemsInLevel = 4;
    [SerializeField] private int targetStar3Score = 1200;

    [Header("Progress Tracking")]
    private int trashCleaned = 0;
    private int neededItemsDiscovered = 0;
    private int correctlyPlacedItems = 0;
    private int lastEvaluatedScore = -1;

    // Objectives Status
    public bool MinorTrashComplete => trashCleaned >= totalTrashInLevel;
    public bool MinorNeededItemsComplete => neededItemsDiscovered >= totalNeededItemsInLevel;
    public bool AllMinorObjectivesComplete => MinorTrashComplete && MinorNeededItemsComplete;

    public bool Star1Earned { get; private set; }
    public bool Star2Earned { get; private set; }
    public bool Star3Earned { get; private set; }

    public event Action OnObjectivesUpdated;
    public event Action<int> OnStarEarned; // Passes star index (1, 2, or 3)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (scoreDisplay == null)
        {
            scoreDisplay = FindFirstObjectByType<ScoreDisplay>();
        }
    }

    private void Start()
    {
        EvaluateObjectives();
    }

    private void Update()
    {
        // Actively monitor score changes to trigger Star 3 instantly
        int currentScore = GetCurrentScore();
        if (currentScore != lastEvaluatedScore)
        {
            lastEvaluatedScore = currentScore;
            EvaluateObjectives();
        }
    }

    public void ReportTrashDestroyed()
    {
        trashCleaned++;
        EvaluateObjectives();
    }

    public void ReportNeededItemDiscovered()
    {
        neededItemsDiscovered = Mathf.Min(neededItemsDiscovered + 1, totalNeededItemsInLevel);
        EvaluateObjectives();
    }

    public void ReportPlacementChange(int correctlyPlacedCount)
    {
        correctlyPlacedItems = correctlyPlacedCount;
        EvaluateObjectives();
    }

    public void EvaluateObjectives()
    {
        int currentScore = GetCurrentScore();

        // Star 1: Complete all minor objectives
        if (!Star1Earned && AllMinorObjectivesComplete)
        {
            Star1Earned = true;
            OnStarEarned?.Invoke(1);
        }

        // Star 2: Place all needed items in respective placement zones
        if (!Star2Earned && correctlyPlacedItems >= totalNeededItemsInLevel)
        {
            Star2Earned = true;
            OnStarEarned?.Invoke(2);
        }

        // Star 3: Reach target score threshold
        if (!Star3Earned && currentScore >= targetStar3Score)
        {
            Star3Earned = true;
            OnStarEarned?.Invoke(3);
        }

        OnObjectivesUpdated?.Invoke();
    }

    private int GetCurrentScore()
    {
        if (scoreDisplay == null)
        {
            scoreDisplay = FindFirstObjectByType<ScoreDisplay>();
        }
        return scoreDisplay != null ? scoreDisplay.CurrentScore : 0;
    }

    // Progress getters for UI text formatting
    public string GetTrashProgressText() => $"{trashCleaned} / {totalTrashInLevel}";
    public string GetNeededItemsProgressText() => $"{neededItemsDiscovered} / {totalNeededItemsInLevel}";
    public string GetPlacementProgressText() => $"{correctlyPlacedItems} / {totalNeededItemsInLevel}";
    public string GetScoreProgressText() => $"{GetCurrentScore()} / {targetStar3Score}";
}