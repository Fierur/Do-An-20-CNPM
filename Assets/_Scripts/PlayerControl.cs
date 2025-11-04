using UnityEngine;
using UnityEngine.UIElements;

//Rảnh thì fix cái tự động crouch

public class PlayerControl : MonoBehaviour
{
    // use [SerializeField] to show private variables in the Unity Inspector
    // or use public to make them accessible from other scripts

    Rigidbody2D rb;
    Animator anim;
    BoxCollider2D boxc;

    [SerializeField] Transform groundCheckCollider;
    [SerializeField] Transform ceilingCheckCollider;
    [SerializeField] LayerMask groundLayer; // Layer for ground detection
    Vector2 groundCheckBoxSize;
    Vector2 ceilingCheckBoxSize;
    float groundCheckAngle = 0f; // To store the angle of the slope
    //const float ceilingCheckRadius = 0.2f;

    float worldWidth;
    float worldHeight;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float jumpPower = 300f;
    [SerializeField] float fallAcceleration = 0.5f;
    float moveSpeedModifier = 2f;
    float crouchSpeedModifier = 0.5f;
    float horizontalValue;//For A/D or Left/Right input
    bool facingRight = true; // To track the player's facing direction
    bool isRunning;
    [SerializeField] bool isGrounded;
    [SerializeField] bool jump;//another flag to check if the player can jump
    bool crouchPressed;
    //float currentVelocity = 0f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxc = GetComponent<BoxCollider2D>();

        if (boxc != null)
        {
            worldWidth = boxc.size.x * transform.localScale.x;
            worldHeight = boxc.size.y * transform.localScale.y;

            groundCheckBoxSize = new Vector2(worldWidth * 0.95f, 0.3f);
            ceilingCheckBoxSize = new Vector2(worldWidth * 0.8f, 0.25f);
        }
    }
    void Start()
    {

    }

    void Update()
    {
        // If the player cannot move, exit the Update method
        if (CanMove() == false)
            return;
        //Get the horizontal input(A/D or Left/Right)
        horizontalValue = Input.GetAxis("Horizontal");
        //when release A or D key, trigger running_end animation
        //running_end animation only trigger when player 
        // is in running_loop animation
        if ((Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D)) && anim.GetCurrentAnimatorStateInfo(0).IsName("Stand_Moving")
        && anim.GetFloat("xVelocity") >= 9.8f)
        {
            isRunning = false;
            anim.SetTrigger("runEnd");
        }

        #region The Explanation of the if command below
        //In if command below, condition is not included "&& isGrounded"
        //because in the Move() method, we added a reference jumpFlag
        //and when call Move() in FixedUpdate(), the bool "jump" is used
        //to check whenever we press the Jump button
        #endregion

        /*We can include the condition "&& isGrounded" below
        but it will not be necessary because we already check*/
        //if press Jump button enable jump
        if (Input.GetButtonDown("Jump"))
        {
            anim.SetBool("Jump", true);
            jump = true;
        }
        //otherwise disable jump
        else if (Input.GetButtonUp("Jump"))
        {
            jump = false;
        }

        //if press Crouch button enable crouch
        if (Input.GetButtonDown("Crouch"))
        {
            crouchPressed = true;
        }
        //otherwise disable crouch
        else if (Input.GetButtonUp("Crouch"))
        {
            crouchPressed = false;
        }

        //Set yVelocity in the Animator Jumping(Blend tree)
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }
    void FixedUpdate()
    {
        GroundCheck();
        if (CanMove() == true)
        {
            Move(horizontalValue, jump, crouchPressed);
            //Update running state for the next frame
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop movement if cannot move
            anim.SetFloat("xVelocity", 0f);
            anim.SetFloat("yVelocity", 0f);
        }

    }

    void OnDrawGizmosSelected()
    {
        // Ensure sizes are available in Editor (Awake may not have run)
        if (groundCheckBoxSize == Vector2.zero || ceilingCheckBoxSize == Vector2.zero)
        {
            BoxCollider2D bc = boxc != null ? boxc : GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                float worldWidth = bc.size.x * transform.localScale.x;
                if (groundCheckBoxSize == Vector2.zero)
                    groundCheckBoxSize = new Vector2(worldWidth * 0.95f, 0.3f);
                if (ceilingCheckBoxSize == Vector2.zero)
                    ceilingCheckBoxSize = new Vector2(worldWidth * 0.8f, 0.25f);
            }
        }

        if (groundCheckCollider != null)
        {
            Gizmos.color = Color.hotPink;
            Gizmos.DrawWireCube(groundCheckCollider.position, groundCheckBoxSize);
        }

        if (ceilingCheckCollider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(ceilingCheckCollider.position, ceilingCheckBoxSize);
        }

    }

    bool CanMove()
    {
        bool canMove = true;
        // Check if the examine window is open
        if (FindAnyObjectByType<InteractionSystem>().isExamineWindowOpen)
            canMove = false;
        // Check if the inventory is open
        if (FindAnyObjectByType<InventorySystem>().isInventoryOpen)
            canMove = false;

        return canMove;
    }
    void GroundCheck()
    {
        isGrounded = false;
        //check if the groundCheckObject is colliding with other
        //2D Colliders that are in the "Ground" layer
        //if yes (isGrounded = true) else (isGrounded = false)
        //Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckCollider.position, groundCheckRadius, groundLayer); //This groundCheck is a circle
        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheckCollider.position, groundCheckBoxSize, groundCheckAngle, groundLayer);
        if (colliders.Length > 0)
            isGrounded = true;

        //As long as player grounded, "Jump" bool parameter 
        //in the Animator is disabled
        anim.SetBool("Jump", !isGrounded);
    }
    //Reset crouch state from other scripts
    public void ResetCrouch()
    {
        crouchPressed = false;
    }

    void Move(float dir, bool jumpFlag, bool crouchFlag)
    //dir = direction, and jumpFlag is used to double check if the player can jump
    {
        #region Jump & Crouch

        /*If crouching and disable crouching
        Check overhead(ceiling) for collision with Ground item
        If there are any, remain crouching, otherwise un-crouch*/
        if (Physics2D.OverlapBoxAll(ceilingCheckCollider.position, ceilingCheckBoxSize, groundCheckAngle, groundLayer).Length > 0)
        {
            // If there is a ceiling above, remain crouching
            crouchFlag = true;
        }


        if (!crouchFlag && jumpFlag)
        {
            if (isGrounded)
            {
                jumpFlag = false;
                //isGrounded = false;
                //Add jump force to the Rigidbody2D
                rb.AddForce(new Vector2(0f, jumpPower));
            }
        }


        //Add Fall Accelation Speed
        if (rb.linearVelocity.y < 0)
        {
            // Apply a downward force to increase fall speed
            //0.25f is equal to increase the fall speed by 25%
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * fallAcceleration * Time.fixedDeltaTime;
        }

        anim.SetBool("Crouch", crouchFlag);
        anim.SetTrigger("CrouchCancel");
        #endregion

        #region Move & Run
        // Adjusted for frame rate independence
        //using dir and moveSpeed to calculate the xValue
        float xValue = dir * moveSpeed * 100 * Time.fixedDeltaTime;
        if (isRunning)
        {
            xValue *= moveSpeedModifier;
        }

        if (crouchFlag)
        {
            xValue *= crouchSpeedModifier;
        }

        // If no input and grounded, stop horizontal movement to prevent sliding
        if (Mathf.Abs(dir) < 0.1f && isGrounded)
        {
            // Stop horizontal movement but keep vertical velocity
            Vector2 targetVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            /* acceleration by MoveTowards. 20f is the acceleration speed, 
            // by decreasing this value, the acceleration will be slower
            currentVelocity = Mathf.MoveTowards(currentVelocity, xValue, 20f * Time.fixedDeltaTime);
            */
            //if using MoveTowards, change the line below from "xValue" to "currentVelocity"
            Vector2 targetVelocity = new Vector2(xValue, rb.linearVelocity.y);
            rb.linearVelocity = targetVelocity;
        }


        //if facing right & press left, flip to the left
        if (facingRight && dir < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            facingRight = false;
        }
        //if facing left & press right, flip to the right
        else if (!facingRight && dir > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            facingRight = true;
        }
        // Set the float xVelocity according to the x value of 
        // the RigidBody2D velocity
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        #endregion

        //Debug.Log("xVelocity: " + rb.linearVelocity.x);  
        //Debug.Log("yVelocity: " + rb.linearVelocity.y);

    }



}
