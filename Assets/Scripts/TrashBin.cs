using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoreDisplay;
    [SerializeField] private int trashPoints = 500;
    [SerializeField] private int neededPoints = -500;
    [SerializeField] private int neutralPoints = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        if (other.TryGetComponent(out InteractableItem item))
        {
            int pointChange = 0;

            switch (item.Tag)
            {
                case ItemTag.Trash:
                    pointChange = trashPoints;
                    break;
                case ItemTag.Needed:
                    pointChange = neededPoints;
                    break;
                case ItemTag.Neutral:
                    pointChange = neutralPoints;
                    break;
            }

            if (scoreDisplay != null && pointChange != 0)
            {
                scoreDisplay.AddScore(pointChange);
            }

            Debug.Log($"Trashed: {other.name} [{item.Tag}] | Points: {pointChange}");
            Destroy(other.gameObject);
        }
    }
}