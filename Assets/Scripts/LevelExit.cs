using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [Header("References")]
    public ScoreDisplay scoreDisplay;
    public LevelEvaluationUI evaluationUI;

    [Header("Star Score Thresholds")]
    public int star1Threshold = 250;
    public int star2Threshold = 500;
    public int star3Threshold = 1000;

    private bool playerInTrigger = false;

    void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            EvaluateAndExit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }

    private void EvaluateAndExit()
    {
        int currentScore = scoreDisplay != null ? scoreDisplay.CurrentScore : 0;

        int starsEarned = 0;
        if (currentScore >= star3Threshold) starsEarned = 3;
        else if (currentScore >= star2Threshold) starsEarned = 2;
        else if (currentScore >= star1Threshold) starsEarned = 1;

        if (evaluationUI != null)
        {
            evaluationUI.ShowEvaluation(currentScore, starsEarned);
        }
    }
}