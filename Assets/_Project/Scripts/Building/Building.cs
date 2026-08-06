using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public abstract class Building : MonoBehaviour
{
    public GridCoord gridPosition;
    public GridDirection facing;
    public BuildingDefinition definition;

    protected Dictionary<ItemType, int> inputBuffer = new();
    protected Dictionary<ItemType, int> outputBuffer = new();
    public IReadOnlyDictionary<ItemType, int> InputBufferView => inputBuffer;
    public IReadOnlyDictionary<ItemType, int> OutputBufferView => outputBuffer;
    protected string currentRecipeID;

    public virtual float ProcessingProgress => 0f;
    public virtual bool IsProcessing => false;

    public void Initialize(GridCoord position, GridDirection direction, BuildingDefinition def)
    {
        gridPosition = position;
        facing = direction;
        definition = def;
        transform.position = new Vector3(position.x, 0, position.z);
    }

    protected bool IsInputSide(GridDirection worldDir)
    {
        GridDirection relativeDir = WorldToRelative(worldDir);
        return definition.inputDirections.Contains(relativeDir);
    }

    protected bool IsOutputSide(GridDirection worldDir)
    {
        GridDirection relativeDir = WorldToRelative(worldDir);
        return definition.outputDirections.Contains(relativeDir);
    }
    
    protected GridDirection WorldToRelative(GridDirection worldDir)
    {
        return (GridDirection)(((int)worldDir - (int)facing + 4) % 4);
    }
    
    protected GridDirection RelativeToWorld(GridDirection relativeDir)
    {
        return (GridDirection)(((int)relativeDir + (int)facing + 4) % 4);
    }

    public virtual Dictionary<ItemType, int> ClearInputBuffer()
    {
        var copy = new Dictionary<ItemType, int>(inputBuffer);
        inputBuffer.Clear();
        return copy;
    }

    public abstract void OnTick(float deltaTime);
}
