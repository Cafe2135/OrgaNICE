using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoreDisplay;

    private void OnTriggerEnter(Collider other)
    {
        TryProcessTrash(other);
    }

    private void TryProcessTrash(Collider other)
    {
        if (other.CompareTag("Player")) return;

        if (other.TryGetComponent(out InteractableItem item) && item.enabled)
        {
            if (other.TryGetComponent(out Rigidbody rb))
            {
                PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
                if (playerInteraction != null && playerInteraction.IsHolding && playerInteraction.HeldBody == rb)
                {
                    return;
                }
            }

            item.enabled = false;

            int pointChange = 0;
            switch (item.Tag)
            {
                case ItemTag.Trash:
                    pointChange = item.TrashPoints;
                    // Notify Objective Manager on trash destruction
                    if (LevelObjectiveManager.Instance != null)
                    {
                        LevelObjectiveManager.Instance.ReportTrashDestroyed();
                    }
                    break;
                case ItemTag.Needed:
                    pointChange = -item.TrashPoints;
                    break;
                case ItemTag.Neutral:
                    pointChange = 0;
                    break;
            }

            if (scoreDisplay != null && pointChange != 0)
            {
                scoreDisplay.AddScore(pointChange);
            }

            // Trigger objective updates after score changes
            if (LevelObjectiveManager.Instance != null)
            {
                LevelObjectiveManager.Instance.EvaluateObjectives();
            }

            Destroy(other.gameObject);
        }
    }
}