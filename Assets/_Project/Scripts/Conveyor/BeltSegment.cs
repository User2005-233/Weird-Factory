using System.Collections.Generic;
using UnityEngine;

public class BeltSegment
{
    public GridCoord start;
    public GridCoord end;
    public GridDirection direction;
    public float length;
    public float speed = 2f;
    public List<BeltItem> items = new List<BeltItem>();
    public const float ItemSpacing = 0.5f;

    public bool TryPushItem(BeltItem item)
    {
        if (!CanAcceptAtStart()) return false;
        items.Insert(0, new BeltItem { itemType = item.itemType, progress = 0f });
        return true;
    }

    public bool CanAcceptAtStart()
    {
        if (items.Count == 0) return true;
        return items[0].progress >= ItemSpacing;
    }

    public void Tick(float interval)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            item.progress += speed * interval;

            if (i == items.Count - 1)
            {
                if (item.progress > length)
                    item.progress = length;
            }
            else
            {
                float maxProgress = items[i + 1].progress - ItemSpacing;
                if (item.progress > maxProgress)
                    item.progress = maxProgress;
            }
            items[i] = item;
        }
    }
}
