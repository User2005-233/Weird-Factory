using System.Collections.Generic;

public class BeltLine
{
    public List<BeltSegment> segments = new List<BeltSegment>();
    public IOutputPortProvider outputProvider;
    public IInputPortProvider inputProvider;

    public BeltSegment StartSegment => segments[0];
    public BeltSegment EndSegment => segments[^1];

    public void AddSegment(BeltSegment segment)
    {
        segments.Add(segment);
    }

    public void Tick(float interval)
    {
        if (segments.Count == 0) return;

        if (outputProvider != null && StartSegment.CanAcceptAtStart())
        {
            if (outputProvider.TryExtract(out ItemType extracted))
                StartSegment.TryPushItem(new BeltItem { itemType = extracted, progress = 0f });
        }

        for (int i = 0; i < segments.Count; i++)
            segments[i].Tick(interval);

        for (int i = segments.Count - 1; i >= 0; i--)
        {
            var seg = segments[i];
            while (seg.items.Count > 0 && seg.items[^1].progress >= seg.length)
            {
                var front = seg.items[^1];
                bool ok;

                if (i < segments.Count - 1)
                    ok = segments[i + 1].TryPushItem(front);
                else if (inputProvider != null)
                    ok = inputProvider.TryInsert(front.itemType);
                else
                    break;

                if (ok)
                    seg.items.RemoveAt(seg.items.Count - 1);
                else
                    break;
            }
        }
    }

    public void ConnectToPorts(IOutputPortProvider output, IInputPortProvider input)
    {
        outputProvider = output;
        inputProvider = input;
    }
}
