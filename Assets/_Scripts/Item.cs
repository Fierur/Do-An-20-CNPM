using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Item : MonoBehaviour
{
    public enum InteractionType { None, PickUp, Examine }
    public enum ItemType { Static, Consumable }
    [Header("Attributes")]

    public InteractionType interactType;
    public ItemType type;
    [Header("Examine")]
    public string descriptionText;
    [Header("Events")]
    public UnityEvent customEvent;
    public UnityEvent consumeEvent;

    void Reset()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
        gameObject.layer = 8; // Set to "Item" layer
    }

    public void Interact()
    {
        switch (interactType)
        {
            case InteractionType.PickUp:
                // Add the item to pickedUpItems list
                var inventory = FindAnyObjectByType<InventorySystem>();
                if (inventory != null)
                {
                    inventory.PickUp(gameObject);
                    // then disable the item in the scene
                    gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning($"[Item] Interact(): No InventorySystem found in scene. Cannot pick up '{gameObject.name}'.");
                }
                break;

            case InteractionType.Examine:
                // Call the ExamineItem method in InteractionSystem
                var interactionSystem = FindAnyObjectByType<InteractionSystem>();
                if (interactionSystem != null)
                {
                    interactionSystem.ExamineItem(this);
                }
                else
                {
                    Debug.LogWarning($"[Item] Interact(): No InteractionSystem found in scene. Cannot examine '{gameObject.name}'.");
                }
                break;
            default:
                // No interaction defined
                break;
        }

        //Invoke (call) the event(s)
        if (customEvent != null)
            customEvent.Invoke();
        else
            Debug.Log("[Item] Interact(): customEvent is null (no callbacks assigned).");

    }
}
