using System.Linq;
using UnityEngine;

public class Storage : Building, IInputPortProvider, IOutputPortProvider, IDirectionProvider
{

    protected int Capacity = 5;
    
    public override bool IsProcessing => outputBuffer.Count > 0;
    
    public override void OnTick(float deltaTime)
    {

    }

    public GridDirection Facing => facing;

    public bool HasInputPort(GridDirection worldDir) => IsInputSide(worldDir);

    public bool HasOutputPort(GridDirection worldDir) => IsOutputSide(worldDir);

    public bool TryInsert(ItemType itemType)
    {
        if(outputBuffer.ContainsKey(itemType))
        {
            if(outputBuffer[itemType] < itemType.maxStackSize)
            {
                outputBuffer[itemType] += 1;
                return true;
            }
            return false;
        }
        else
        {
            if(outputBuffer.Count >= Capacity)
            {
                return false;
            }
            outputBuffer[itemType] = 1;
            return true;
        }
    }

    public bool TryExtract(out ItemType itemType)
    {
        if(outputBuffer.Count > 0)
        {
            itemType = outputBuffer.Keys.Last();
            outputBuffer[itemType] -= 1;
            if(outputBuffer[itemType] <= 0)
            {
                outputBuffer.Remove(itemType);
            }
            return true;
        }
        itemType = null;
        return false;
    }
}
