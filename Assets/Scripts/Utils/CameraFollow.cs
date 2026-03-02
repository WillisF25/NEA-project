using UnityEngine;

/// <summary>
/// Smoothly tracks the leading creature to keep them in focus during the race.
/// </summary>
public class CameraFollow : MonoBehaviour
{   
    [Header("Follow Settings")]
    [Tooltip("Higher values make the camera snap faster, lower values make it slower.")]
    public float smoothSpeed = 5f;
    [Tooltip("The camera's distance and height relative to the creature.")]
    public Vector3 offset = new Vector3(-8, 2, -10); // standard 2D offset (negative z)

    /// <summary>
    /// LateUpdate runs after all regular Updates. This ensures the creature has 
    /// finished moving for the frame before the camera calculates its new position.
    /// </summary>
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
