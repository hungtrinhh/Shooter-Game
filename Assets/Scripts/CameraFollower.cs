using UnityEngine;

public class CameraFollower: MonoBehaviour {
    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offsetPosition;

    Vector3 desiredPosition;

    void LateUpdate () {
        if(target != null)
            desiredPosition = target.position + offsetPosition;
        Vector3 smoothedPosition = Vector3.Lerp (transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
