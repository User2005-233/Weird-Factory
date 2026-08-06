
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Factory/ItemType")]
public class ItemType : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    public GameObject meshPrefab;
    public int maxStackSize;
}