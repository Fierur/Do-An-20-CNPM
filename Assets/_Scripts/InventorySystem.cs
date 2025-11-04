using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    [Header("General Fields")]
    //Assign the "Content" GameObject here in the Inspector
    [SerializeField] Transform slotParent;
    //In the "_Prefabs" folder, drag the prefab file of the slot here
    [SerializeField] GameObject slotPrefab;
    [SerializeField] int slotCount = 32;

    //List of picked up items
    public List<GameObject> items;
    //flag to check if the inventory is open
    public bool isInventoryOpen;
    [Header("UI Items Fields")]
    //Inventory system UI Window
    public GameObject ui_Window;
    // Array of images to display items
    public Image[] items_Images; 
    [Header("UI Item Description")]
    public GameObject ui_Description_Window;
    public Image description_Image;
    public TMP_Text name_Text;
    public TMP_Text description_Text;
    // To track the selected item ID,
    // by default -1 means no item selected
    public int selectedItemId = -1;

    void Awake()
    {
        //If want to keep manual slots at the beginning of the list,
        //create automatic slots first, the call AutoAddImageIntoItemsImages
        //to get the correct order of slots in Content
        AutoAddSlotPrefabs();
        AutoAddImageIntoItemsImages();
        AssignButtonEvents();

    }

    void Update()
    {
        //Check if the inventory key is pressed and the examine window is not open 
        if (Input.GetKeyDown(KeyCode.I) && !FindAnyObjectByType<InteractionSystem>().isExamineWindowOpen)
        {
            ToggleInventory();
        }
        HandleConsumeInput();
        HandleEscapeInput();
    }
    
    void HandleEscapeInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Đóng inventory nếu đang mở
            if (isInventoryOpen)
            {
                isInventoryOpen = false;
                ui_Window.SetActive(false);
            }
            //if have more UI windows, write here to close them
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        ui_Window.SetActive(isInventoryOpen);
        //Refresh the UI to show the items in the inventory
        Update_UI();
    }
    //Add the item to the "items" list 
    public void PickUp(GameObject item)
    {
        items.Add(item);
        Update_UI();
    }

    //Refresh the UI to show the items in the inventory
    void Update_UI()
    {
        //Hide all the items first
        HideAll();
        //For each item in the "items" list
        //Show it in the respective slot in the "items_Images" array
        for (int i = 0; i < items.Count; i++)
        {
            items_Images[i].sprite = items[i].GetComponent<SpriteRenderer>().sprite;
            items_Images[i].gameObject.SetActive(true);
        }
    }

    //Hide all the items images in the UI
    void HideAll()
    {
        foreach (var i in items_Images)
        {
            i.gameObject.SetActive(false);
        }
    }

    public void ShowDescription(int id)
    {
        //Set the image
        description_Image.sprite = items[id].GetComponent<SpriteRenderer>().sprite;
        //Set the name
        name_Text.text = items[id].name;
        //Set the description
        description_Text.text = items[id].GetComponent<Item>().descriptionText;
        //Show the elements
        description_Image.gameObject.SetActive(true);
        name_Text.gameObject.SetActive(true);
        description_Text.gameObject.SetActive(true);
        //Set the selected item ID
        selectedItemId = id;
    }

    public void HideDescription()
    {
        description_Image.gameObject.SetActive(false);
        name_Text.gameObject.SetActive(false);
        description_Text.gameObject.SetActive(false);
        //Reset the selected item ID
        selectedItemId = -1;
    }

    void AutoAddImageIntoItemsImages()
    {
        // Take all Image components from the slotParent's children
        int slotCount = slotParent.childCount;
        items_Images = new Image[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            Transform slot = slotParent.GetChild(i);
            //Find GameObject with the name "Image" under each slot
            var imageObj = slot.Find("Image");
            if (imageObj != null)
                items_Images[i] = imageObj.GetComponent<Image>();
        }
    }

    void AutoAddSlotPrefabs()
    {
        items_Images = new Image[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            slot.name = "Item" + (i + 1);
            //Find child Image component in the slot
            var imageObj = slot.transform.Find("Image");
            if (imageObj != null)
                items_Images[i] = imageObj.GetComponent<Image>();
            else
                // Hide slot if no Image found
                slot.SetActive(false);
        }
    }

    void AssignButtonEvents()
    {
        for (int i = 0; i < items_Images.Length; i++)
        {
            //Find the Button component in each slot
            Button btn = items_Images[i].GetComponent<Button>();
            // Capture the current index
            int index = i;
            if (btn != null)
            {
                //Avoid adding multiple listeners
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowDescription(index));
            }
        }
    }

    public void Consume(int id)
    {
        if (items[id].GetComponent<Item>().type == Item.ItemType.Consumable)
        {
            Debug.Log("Consuming item: " + items[id].name);
            //Invoke the consume event
            items[id].GetComponent<Item>().consumeEvent.Invoke();
            //Destroy the item from the inventory in a short delay
            Destroy(items[id], 0.1f);
            //Remove the item from the inventory list
            items.RemoveAt(id);
            //Update the UI to reflect the changes
            Update_UI();
        }
    }

    void HandleConsumeInput()
    {
        //Consume item when inventory is open
        //the item got selected by clicking on it
        //if there is a consumable item, press 'E' to consume it
        if (isInventoryOpen && selectedItemId != -1 && Input.GetKeyDown(KeyCode.E))
        {
            Consume(selectedItemId);
            HideDescription();
        }
    }
}
