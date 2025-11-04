using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Detection Fields")]
    public Transform detectionPoint;
    private const float detectionRadius = 0.2f;
    public LayerMask detectionLayer;
    //Cached object triggered by the detection
    //This is used to check if the player can interact with the object
    public GameObject detectedObject;

    [Header("Examine Fields")]
    public GameObject examineWindow;
    public Image examineImage;
    public TMP_Text examineText;
    public bool isExamineWindowOpen;


    void OnDrawGizmosSelected()
    {
        // Draw a sphere at the detection point
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);
    }

    // Update is called once per frame
    void Update()
    {
        //Do not allow open inventory if examine window is open
        //This is to prevent the player from opening the inventory while examining an item
        if (FindAnyObjectByType<InventorySystem>().isInventoryOpen)
            return; 
            
        // Allow closing the examine window
        if (isExamineWindowOpen)
        {
            if (InteractInput() || Input.GetKeyDown(KeyCode.Escape))
            {
                //Hide the examine window
                examineWindow.SetActive(false);
                isExamineWindowOpen = false;
                //Reset crouch state if the player interact with the item 
                //while crouching, the player will stand up after closing the examine window
                FindAnyObjectByType<PlayerControl>().ResetCrouch();
            }
            return; //if the examine window is open, skip the rest of the Update
        }

        // Check for interaction input
            if (DetectObject())
            {
                if (InteractInput())
                {
                    //just ignore this comment, it is just a placeholder.
                    // "This sword is the rightful possession of the Hero King! You are unworthy to wield it."
                    detectedObject.GetComponent<Item>().Interact();
                }
            }
    }

    bool InteractInput()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    bool DetectObject()
    {
        Collider2D obj = Physics2D.OverlapCircle(detectionPoint.position, detectionRadius, detectionLayer);
        if (obj == null)
        {
            detectedObject = null;
            return false;
        }
        else
        {
            detectedObject = obj.gameObject;
            return true;
        }

    }

    public void ExamineItem(Item item)
    {
        if (isExamineWindowOpen)
        {
            //Hide the examine window
            examineWindow.SetActive(false);
            //Disable the examine window to embed the CanMove() function 
            //to allow player movement while examining
            isExamineWindowOpen = false;
        }
        else
        {
            //Show the image's item in the middle of window
            examineImage.sprite = item.GetComponent<SpriteRenderer>().sprite;
            //Write the description text under the image
            examineText.text = item.descriptionText;
            //Display the examine window
            examineWindow.SetActive(true);
            //Enable the examine window to embed the CanMove() function 
            //to restrict player movement while examining
            isExamineWindowOpen = true;
        }
        
    }
}
