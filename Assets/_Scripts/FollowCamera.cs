using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    [Range(1, 15)]
    public float smoothFactor;
    public float cameraZoomScale = 10f;
    public Vector3 minBoundValue, maxBoundValue;

    // FixedUpdate is called at a fixed time interval and is used for physics calculations
    void Start()
    {
        Camera.main.orthographicSize = cameraZoomScale;
    }
    void FixedUpdate()
    {
        Follow();
    }

    void Follow()
    {
        Vector3 targetCameraPos = target.position + offset;
        //Verify if the targetCameraPos is out of bound or not
        //Limit it to the min and max values
        Vector3 cameraBoundPos = new Vector3(
            Mathf.Clamp(targetCameraPos.x, minBoundValue.x, maxBoundValue.x),
            Mathf.Clamp(targetCameraPos.y, minBoundValue.y, maxBoundValue.y),
            Mathf.Clamp(targetCameraPos.z, minBoundValue.z, maxBoundValue.z));

        Vector3 smoothPosition = Vector3.Lerp(transform.position, cameraBoundPos, smoothFactor * Time.fixedDeltaTime);
        transform.position = smoothPosition;

    }
}
