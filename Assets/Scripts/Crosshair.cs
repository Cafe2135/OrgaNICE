using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private float size = 6f;
    [SerializeField] private Color color = Color.white;

    void Awake()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(size, size);

        var image = gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color;
    }
}