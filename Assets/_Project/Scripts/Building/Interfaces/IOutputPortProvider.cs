public interface IOutputPortProvider
{
    bool HasOutputPort(GridDirection worldDir);
    bool TryExtract(out ItemType itemType);
}
