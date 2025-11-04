using UnityEngine;

public class TestIsTriggerFalse : MonoBehaviour
{
    //IsTrigger = false, detect collision events with other colliders but cannot pass through
    //This section is for testing purpose only
    [SerializeField] LayerMask collisionLayer; // Lớp vật thể muốn phản ứng
    [SerializeField] bool debugLog = true;     // Bật log nếu muốn kiểm tra

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, collisionLayer))
        {
            if (debugLog)
                Debug.Log($"[Collision Enter] Player va chạm với: {collision.gameObject.name}");

            // Ví dụ: nếu chạm tường thì phản lực, hất ngược, v.v.
            // Hoặc thông báo cho PlayerControl thông qua GetComponentInParent<PlayerControl>()
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, collisionLayer))
        {
            if (debugLog)
                Debug.Log($"[Collision Stay] Player đang chạm với: {collision.gameObject.name}");
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, collisionLayer))
        {
            if (debugLog)
                Debug.Log($"[Collision Exit] Player rời khỏi: {collision.gameObject.name}");
        }
    }

    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

}
