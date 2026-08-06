public interface IInputPortProvider
{
    bool HasInputPort(GridDirection worldDir);
    bool TryInsert(ItemType itemType);
}
