using UnityEngine;

public class TrashBin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        if (other.TryGetComponent(out InteractableItem item))
        {
            Debug.Log($"Trashed: {other.name} [{item.Tag}]");
            Destroy(other.gameObject);
        }
    }
}