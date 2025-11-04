using UnityEngine;

public class TestFrictionAndBounciness : MonoBehaviour
{
    [Header("Friction & Bounciness")]
    [Range(0f, 1f)] public float friction = 0.4f;
    [Range(0f, 1f)] public float bounciness = 0.1f;

    [Tooltip("Tự động cập nhật vật liệu mỗi khi chỉnh thông số trong Inspector")]
    public bool autoUpdate = true;

    PhysicsMaterial2D dynamicMaterial;

    void Awake()
    {
        // Tạo material runtime, không ảnh hưởng asset gốc
        dynamicMaterial = new PhysicsMaterial2D($"{gameObject.name}_RuntimeMaterial");
        ApplyMaterial();
    }

    void OnValidate()
    {
        if (autoUpdate && Application.isPlaying && dynamicMaterial != null)
        {
            ApplyMaterial();
        }
    }

    void ApplyMaterial()
    {
        dynamicMaterial.friction = friction;
        dynamicMaterial.bounciness = bounciness;

        // Áp dụng cho Collider2D hiện tại
        var col = GetComponent<Collider2D>();
        col.sharedMaterial = dynamicMaterial;
    }

    // Nếu muốn chỉnh trong runtime (vd. player trượt trên băng)
    public void SetPhysicsProperties(float newFriction, float newBounciness)
    {
        friction = Mathf.Clamp01(newFriction);
        bounciness = Mathf.Clamp01(newBounciness);
        ApplyMaterial();
    }
}
