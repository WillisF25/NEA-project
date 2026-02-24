using UnityEngine;

public class CameraFollow : MonoBehaviour
{   
    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(-8, 2, -10); // standard 2D offset (negative z)

void LateUpdate()
    {
        // do nothing if no target
        if (SimulationManager.focusTarget == null) return;

        // where camera want to be
        Vector3 desiredPosition = SimulationManager.focusTarget.position + offset;
        
        // lerp from current position to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        smoothedPosition.y = 0; // keep vertical locked
        
        // apply position
        transform.position = smoothedPosition;
    }
}
