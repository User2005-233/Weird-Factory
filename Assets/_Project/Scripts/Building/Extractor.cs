using System.Collections.Generic;

public class Extractor : Building, IOutputPortProvider, IDirectionProvider
{
    readonly float speedInterval = 2f;
    ItemType resourceType;
    Timer extractTimer;

    public override bool IsProcessing => outputBuffer.Count < resourceType.maxStackSize;

    public void SetResourceType(ItemType type)
    {
        resourceType = type;
        outputBuffer.Clear();
        extractTimer = Timer.Register(speedInterval, ExtractToBuffer, true);
        extractTimer.Start();
    }

    public override void OnTick(float deltaTime)
    {
        extractTimer.Tick(deltaTime);
    }

    void ExtractToBuffer()
    {
        if(outputBuffer[resourceType] < resourceType.maxStackSize)
            outputBuffer[resourceType] += 1;
    }

    public GridDirection Facing => facing;

    public bool HasOutputPort(GridDirection worldDir)
    {
        return IsOutputSide(worldDir);
    }

    public bool TryExtract(out ItemType itemType)
    {
        itemType = resourceType;
        if(resourceType != null && outputBuffer.GetValueOrDefault(resourceType, 0) > 0)
        {
            outputBuffer[resourceType] -= 1;
            return true;
        }
        itemType = null;
        return false;
    }
}
