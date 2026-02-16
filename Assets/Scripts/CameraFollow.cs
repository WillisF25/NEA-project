using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    void LateUpdate() {
        if (SimulationManager.focusTarget != null) {
            transform.position = new Vector3(SimulationManager.focusTarget.position.x, 0, -10);
        }
    }
}
