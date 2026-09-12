using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoreDisplay;

    private void OnTriggerEnter(Collider other)
    {
        TryProcessTrash(other);
    }

    private void OnTriggerStay(Collider other)
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
                    pointChange = item.BasePoints;
                    break;
                case ItemTag.Needed:
                    pointChange = -item.BasePoints;
                    break;
                case ItemTag.Neutral:
                    pointChange = 0;
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