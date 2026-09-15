using System.Collections.Generic;
using UnityEngine;

public enum ZoneType { Table, Drawer }

public class PlacementZone : MonoBehaviour
{
    [SerializeField] private ScoreDisplay scoreDisplay;
    [SerializeField] private ZoneType zoneType = ZoneType.Table;

    private readonly List<InteractableItem> itemsInZone = new List<InteractableItem>();
    private int currentZoneScore = 0;

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

    private void RecalculateScore()
    {
        int newScore = 0;

        foreach (var item in itemsInZone)
        {
            if (item == null || !item.gameObject.activeInHierarchy) continue;

            if (zoneType == ZoneType.Drawer && !item.IsInStorage) continue;

            int basePoints = (zoneType == ZoneType.Table) ? item.TablePlacementPoints : item.DrawerPlacementPoints;

            switch (item.Tag)
            {
                case ItemTag.Needed: newScore += basePoints; break;
                case ItemTag.Trash: newScore -= basePoints; break;
                case ItemTag.Neutral: break;
            }
        }

        int scoreDelta = newScore - currentZoneScore;
        currentZoneScore = newScore;

        if (scoreDisplay != null && scoreDelta != 0)
        {
            scoreDisplay.AddScore(scoreDelta);
        }
    }
}