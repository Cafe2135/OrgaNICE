using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private int score = 0;
    [SerializeField] private int digits = 5;
    [SerializeField] private int fontSize = 36;
    [SerializeField] private Color textColor = Color.white;

    private TMP_Text label;

    void Awake()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);
        rect.sizeDelta = new Vector2(200f, 50f);

        label = gameObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAlignmentOptions.MidlineRight;

        Refresh();
    }

    public void SetScore(int newScore)
    {
        score = newScore;
        Refresh();
    }

    public void AddScore(int amount)
    {
        SetScore(score + amount);
    }

    private void Refresh()
    {
        if (label == null) return;

        if (score < 0)
        {
            label.text = "-" + Mathf.Abs(score).ToString().PadLeft(digits - 1, '0');
        }
        else
        {
            label.text = score.ToString().PadLeft(digits, '0');
        }
    }
}