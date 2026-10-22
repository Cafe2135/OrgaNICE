using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private float interactRange = 3.5f;
    [SerializeField] private Camera playerCamera;

    void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (PauseMenu.IsPaused || LevelEvaluationUI.IsEvaluating || StorageInteractionController.IsInStorageMode || ChecklistUI.IsChecklistOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryExit();
        }
    }

    private void TryExit()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (LevelEvaluationUI.Instance != null)
                {
                    LevelEvaluationUI.Instance.ShowEvaluation();
                }
            }
        }
    }
}