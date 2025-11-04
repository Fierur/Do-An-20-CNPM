using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [System.Serializable]
    public class Waypoint
    {
        public Transform point;
        [Tooltip("Seconds to wait when arriving at this point")]
        public float waitTime = 0f;
    }

    [Header("Waypoints")]
    public List<Waypoint> points = new List<Waypoint>();

    [Header("Patrol")]
    public int nextID = 0;
    int idChangeValue = 1; // +1 or -1 to bounce between ends
    public float speed = 2f;
    public float reachThreshold = 0.15f; //set this value to something small like 0.1 or 0.15
    public bool useDynamicMovement = true; // if true, use rb.velocity so gravity applies
    public float gravityScale = 1f;
    public float fallAcceleration = 0.5f; // extra fall accel multiplier (like player)

    [Header("Environment Checks")]
    public Transform frontCheck;
    public float frontCheckRadius = 0.12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Animator")]
    Animator anim;
    Rigidbody2D rb;

    // Internal state
    enum State { Moving, Waiting }
    State state = State.Moving;

    private void Reset()
    {
        Init();
    }

    // Create default structure and two points if none exist
    void Init()
    {
        GetComponent<Collider2D>().isTrigger = true;

        GameObject root = new GameObject(name + "_Enemy_Root");
        root.transform.position = transform.position;
        transform.SetParent(root.transform);

        GameObject waypoints = new GameObject("Waypoints");
        waypoints.transform.SetParent(root.transform);
        waypoints.transform.position = root.transform.position;

        GameObject p1 = new GameObject("Point1");
        p1.transform.SetParent(waypoints.transform);
        p1.transform.position = root.transform.position;

        GameObject p2 = new GameObject("Point2");
        p2.transform.SetParent(waypoints.transform);
        p2.transform.position = root.transform.position;

        points = new List<Waypoint>
        {
            new Waypoint{ point = p1.transform, waitTime = 0f },
            new Waypoint{ point = p2.transform, waitTime = 0f }
        };
    }

    // Editor-friendly: add a new waypoint child with incremental name
    [ContextMenu("Add Waypoint")]
    public void AddWaypoint()
    {
        // Find (or create) parent Waypoints under the root
        Transform root = transform.parent;
        if (root == null)
        {
            GameObject rootGO = new GameObject(name + "_Enemy_Root");
            rootGO.transform.position = transform.position;
            transform.SetParent(rootGO.transform);
            root = transform.parent;
        }

        Transform waypointsParent = root.Find("Waypoints");
        if (waypointsParent == null)
        {
            GameObject wp = new GameObject("Waypoints");
            wp.transform.SetParent(root);
            wp.transform.position = root.position;
            waypointsParent = wp.transform;
        }

        // Determine next numeric suffix by scanning existing children named PointN
        int maxIndex = 0;
        foreach (Transform child in waypointsParent)
        {
            string name = child.name;
            if (name.StartsWith("Point"))
            {
                string num = name.Substring(5);
                if (int.TryParse(num, out int n))
                    maxIndex = Mathf.Max(maxIndex, n);
            }
        }
        int newIndex = maxIndex + 1;
        GameObject newPoint = new GameObject("Point" + newIndex);
        newPoint.transform.SetParent(waypointsParent);
        newPoint.transform.position = transform.position;

        // Add to serialized points list
        points.Add(new Waypoint { point = newPoint.transform, waitTime = 0f });
        
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        // fallback: if not present (should be added by RequireComponent), add one
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        // sensible defaults
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (useDynamicMovement)
            rb.bodyType = RigidbodyType2D.Dynamic;
        else
            rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnValidate()
    {
        // When user adjusts points in Inspector, avoid modifying hierarchy directly in OnValidate.
        // Instead schedule creation/assignment to the next editor update to prevent SendMessage errors.
        if (Application.isPlaying) return;

        if (points == null) points = new List<Waypoint>();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            // If object destroyed before the delayed call runs, skip
            if (this == null) return;

            Transform root = transform.parent;
            if (root == null)
            {
                GameObject rootGO = new GameObject(name + "_Enemy_Root");
                rootGO.transform.position = transform.position;
                transform.SetParent(rootGO.transform);
                root = transform.parent;
            }

            Transform waypointsParent = root.Find("Waypoints");
            if (waypointsParent == null)
            {
                GameObject wp = new GameObject("Waypoints");
                wp.transform.SetParent(root);
                wp.transform.position = root.position;
                waypointsParent = wp.transform;
            }

            // Ensure enough children exist to match points count
            int needed = points.Count;
            int childCount = waypointsParent.childCount;
            for (int i = childCount; i < needed; i++)
            {
                int nextIndex = i + 1;
                GameObject newPoint = new GameObject("Point" + nextIndex);
                newPoint.transform.SetParent(waypointsParent);
                newPoint.transform.position = transform.position;
            }

            // Assign missing null transforms to corresponding child if available
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] == null)
                    points[i] = new Waypoint();
                if (points[i].point == null)
                {
                    string expectedName = "Point" + (i + 1);
                    Transform namedChild = waypointsParent.Find(expectedName);
                    if (namedChild != null)
                    {
                        points[i].point = namedChild;
                    }
                    else if (i < waypointsParent.childCount)
                    {
                        points[i].point = waypointsParent.GetChild(i);
                    }
                }
            }
        };
#endif
    }

    void Update()
    {
        if (points == null || points.Count == 0) return;
        // Only used for facing and animation smoothing; actual movement done in FixedUpdate via Rigidbody2D
        if (state == State.Moving)
        {
            // update facing based on next target
            Waypoint wp = points[nextID];
            if (wp != null && wp.point != null)
            {
                if (wp.point.position.x > transform.position.x)
                    transform.localScale = new Vector3(-1, 1, 1);
                else
                    transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    void FixedUpdate()
    {
        if (points == null || points.Count == 0) return;
        if (state == State.Moving)
            PerformMoveFixed();
    }

    void PerformMoveFixed()
    {
        Waypoint wp = points[nextID];
        if (wp == null || wp.point == null) return;

        Transform goalPoint = wp.point;
        // Check environment: obstacle ahead or no ground ahead
        bool blocked = false;
        bool hasGroundAhead = true;
        if (frontCheck != null)
        {
            Collider2D c = Physics2D.OverlapCircle(frontCheck.position, frontCheckRadius, groundLayer);
            blocked = c != null;
        }
        if (groundCheck != null)
        {
            Collider2D g = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            hasGroundAhead = g != null;
        }

        if (blocked || !hasGroundAhead)
        {
            // If blocked or would fall, don't move forward: reverse direction or advance index
            // Here we choose to bounce (reverse patrol direction)
            idChangeValue = -idChangeValue;
            nextID = Mathf.Clamp(nextID + idChangeValue, 0, Mathf.Max(0, points.Count - 1));
            state = State.Waiting;
            StartCoroutine(WaitThenAdvance(0.1f)); // short pause before moving again
            if (anim != null)
            {
                anim.SetBool("isMoving", false);
                anim.SetFloat("Speed", 0f);
            }
            return;
        }

        // Movement: use velocity when dynamic so gravity affects the enemy
        if (useDynamicMovement)
        {
            Vector2 dir = (goalPoint.position - transform.position).normalized;
            float vx = dir.x * speed;
            // preserve current vertical velocity (gravity)
            float vy = rb.linearVelocity.y;
            rb.linearVelocity = new Vector2(vx, vy);

            // extra fall acceleration like player
            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * fallAcceleration * Time.fixedDeltaTime;
            }

            if (anim != null)
            {
                anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
                anim.SetBool("isMoving", Mathf.Abs(rb.linearVelocity.x) > 0.05f);
            }
        }
        else
        {
            // Kinematic move by MovePosition
            Vector2 oldPos = rb.position;
            Vector2 targetPos = goalPoint.position;
            Vector2 newPos = Vector2.MoveTowards(oldPos, targetPos, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            // Animator: set isMoving based on actual movement magnitude (using fixedDeltaTime)
            if (anim != null)
            {
                float movedDist = (newPos - oldPos).magnitude;
                float moveSpeed = 0f;
                if (Time.fixedDeltaTime > 0f)
                    moveSpeed = movedDist / Time.fixedDeltaTime;
                anim.SetFloat("Speed", moveSpeed);
                anim.SetBool("isMoving", moveSpeed > 0.05f);
            }
        }

        // Arrived?
        if (Vector2.Distance(rb.position, goalPoint.position) <= reachThreshold)
        {
            // stop and wait according to waypoint's waitTime
            state = State.Waiting;
            if (anim != null)
            {
                anim.SetBool("isMoving", false);
                anim.SetFloat("Speed", 0f);
            }
            // zero horizontal velocity so animation becomes idle while waiting
            if (useDynamicMovement)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            float waitSeconds = Mathf.Max(0f, wp.waitTime);
            StartCoroutine(WaitThenAdvance(waitSeconds));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (frontCheck != null)
            Gizmos.DrawWireSphere(frontCheck.position, frontCheckRadius);

        Gizmos.color = Color.yellow;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    System.Collections.IEnumerator WaitThenAdvance(float seconds)
    {
        if (seconds > 0f)
            yield return new WaitForSeconds(seconds);
        else
            yield return null; // wait one frame to ensure consistent behavior

        // Advance index and bounce
        if (nextID == points.Count - 1)
            idChangeValue = -1;
        else if (nextID == 0)
            idChangeValue = 1;

        nextID += idChangeValue;
        state = State.Moving;
    }
}
