using UnityEngine;

public class ItemCustomTriggerEvent : MonoBehaviour
{
    [Header("Examine item(s) will be effected by other Consumable item(s)")]
    public GameObject[] targetItems;
    [Header("Consumable item(s) that will trigger the effect")]
    public Item[] requiredConsumableItems;
    // Flag to check if one or more than one item has been consumed
    static bool[] consumedFlags;

    void Awake()
    {
        if(requiredConsumableItems != null && requiredConsumableItems.Length >0)
            consumedFlags = new bool[requiredConsumableItems.Length];
    }
    
    public void OnConsume()
    {
        //if there is zero or one item in requiredConsumableItems
        //consume the item and do something
        //else if there are more than one item in requiredConsumableItems
        //consume the item and do something
        //when consume enough items in requiredConsumableItems 
        if (requiredConsumableItems == null || requiredConsumableItems.Length == 0)
        {
            //Consume only one this item then do something
            foreach (var obj in targetItems)
                if (obj != null)
                    obj.SetActive(false);
        }
        else
        {
            //Must consume all items in requiredConsumableItems
            for (int i = 0; i < requiredConsumableItems.Length; i++)
            {
                if (requiredConsumableItems[i] == null)
                    continue;
                if (requiredConsumableItems[i].gameObject == this.gameObject)
                    consumedFlags[i] = true; // Mark this item as consumed
            }
            // Check if all required items have been consumed
            bool allConsumed = true;
            foreach (var flag in consumedFlags)
                if (flag == false)
                    allConsumed = false;
            if (allConsumed)
            {
                // Consume all items and do something
                foreach (var obj in targetItems)
                    if (obj != null)
                        obj.SetActive(false);
            }
        }
    }

}
