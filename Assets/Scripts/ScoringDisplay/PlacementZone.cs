using System.Collections.Generic;
using UnityEngine;

public enum ZoneType { Table, Drawer }

public class PlacementZone : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoreDisplay;
    [SerializeField] private ZoneType zoneType = ZoneType.Table;

    private readonly List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private int currentZoneScore = 0;

    public ZoneType CurrentZoneType => zoneType;

    void Update()
    {
        CleanupAndRecalculate();
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableItem item = other.GetComponentInParent<InteractableItem>();
        if (item != null && !itemsInZone.Contains(item))
        {
            itemsInZone.Add(item);
            RecalculateScore();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableItem item = other.GetComponentInParent<InteractableItem>();
        if (item != null && itemsInZone.Contains(item))
        {
            itemsInZone.Remove(item);
            RecalculateScore();
        }
    }

    private void CleanupAndRecalculate()
    {
        bool changed = false;
        for (int i = itemsInZone.Count - 1; i >= 0; i--)
        {
            if (itemsInZone[i] == null || !itemsInZone[i].gameObject.activeInHierarchy)
            {
                itemsInZone.RemoveAt(i);
                changed = true;
            }
        }

        if (changed) RecalculateScore();
    }

    /// <summary>
    /// Validates if an item is placed in its intended zone based on size and storage state.
    /// </summary>
    public bool IsItemInCorrectZone(InteractableItem item)
    {
        if (item == null || !item.gameObject.activeInHierarchy) return false;

        if (zoneType == ZoneType.Drawer)
        {
            // Small items MUST be in a drawer and snapped into storage
            return item.Size == ItemSize.Small && item.IsInStorage;
        }
        else if (zoneType == ZoneType.Table)
        {
            // Large items MUST be placed on surface/table placement zones
            return item.Size == ItemSize.Large;
        }

        return false;
    }

    private void RecalculateScore()
    {
        int newScore = 0;

        foreach (var item in itemsInZone)
        {
            if (item == null || !item.gameObject.activeInHierarchy) continue;

            int basePoints = (zoneType == ZoneType.Table) ? item.TablePlacementPoints : item.DrawerPlacementPoints;

            switch (item.Tag)
            {
                case ItemTag.Needed:
                    // Only award points if placed in the correct size matching zone
                    if (IsItemInCorrectZone(item))
                    {
                        newScore += basePoints;
                    }
                    break;

                case ItemTag.Trash:
                    // Deduct points for leaving trash on surfaces or inside drawers
                    newScore -= basePoints;
                    break;

                case ItemTag.Neutral:
                    break;
            }
        }

        int scoreDelta = newScore - currentZoneScore;
        currentZoneScore = newScore;

        if (scoreDisplay != null && scoreDelta != 0)
        {
            scoreDisplay.AddScore(scoreDelta);
        }

        // Recalculate total valid placements across all zones in the level for Star 2
        UpdateGlobalObjectiveProgress();
    }

    private void UpdateGlobalObjectiveProgress()
    {
        if (LevelObjectiveManager.Instance == null) return;

        int totalCorrectlyPlaced = 0;
        PlacementZone[] allZones = FindObjectsByType<PlacementZone>(FindObjectsSortMode.None);

        foreach (var zone in allZones)
        {
            foreach (var item in zone.itemsInZone)
            {
                if (item != null && item.gameObject.activeInHierarchy && item.Tag == ItemTag.Needed)
                {
                    if (zone.IsItemInCorrectZone(item))
                    {
                        totalCorrectlyPlaced++;
                    }
                }
            }
        }

        LevelObjectiveManager.Instance.ReportPlacementChange(totalCorrectlyPlaced);
    }
}