using UnityEngine;

public class TestIsTriggerTruePickUp : MonoBehaviour
{
    //IsTrigger = false, detect collision events with other colliders but cannot pass through
    //This section is for testing purpose only
    [SerializeField] LayerMask itemLayer; 
    [SerializeField] bool autoPickup = true; 

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem collider có nằm trong layer item không
        if (IsInLayerMask(other.gameObject.layer, itemLayer))
        {
            var item = other.GetComponent<Item>();
            if (item == null) return;

            // Nếu tự động nhặt
            if (autoPickup && item.interactType == Item.InteractionType.PickUp)
            {
                item.Interact();
            }
            else
            {
                Debug.Log($"[Item Collector] Gặp item: {other.name} (Chọn auto pickup trong Inspector để nhặt)");
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!autoPickup && IsInLayerMask(other.gameObject.layer, itemLayer))
        {
            var item = other.GetComponent<Item>();
            if (item == null) return;

            if (Input.GetKeyDown(KeyCode.E) && item.interactType == Item.InteractionType.PickUp)
            {
                item.Interact();
            }
        }
    }

    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

}
