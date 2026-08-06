using System.Collections.Generic;

public class PlayerInventory:MonoSingleton<PlayerInventory>
{
    Dictionary<ItemType, int> inventory = new();

    public void AddItem(ItemType itemType, int count)
    {
        if(inventory.ContainsKey(itemType))
        {
            inventory[itemType] += count;
        }
        else
        {
            inventory[itemType] = count;
        }
    }

    public void AddItem(Dictionary<ItemType, int> items)
    {
        foreach(var item in items)
        {
            if(inventory.ContainsKey(item.Key))
            {
                inventory[item.Key] += item.Value;
            }
            else
            {
                inventory[item.Key] = item.Value;
            }
        }
    }

    public void RemoveItem(ItemType itemType, int count)
    {
        if(inventory.ContainsKey(itemType))
        {
            inventory[itemType] -= count;
            if(inventory[itemType] <= 0)
            {
                inventory.Remove(itemType);
            }
        }
    }

    public int GetCount(ItemType itemType)
    {
        return inventory.ContainsKey(itemType) ? inventory[itemType] : 0;
    }
}
