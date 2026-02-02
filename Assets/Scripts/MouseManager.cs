using UnityEngine;
using UnityEngine.InputSystem;

public class MouseManager : MonoBehaviour
{
    public Vector3 GetMousePosition()
    {   
        Vector2 mousePos = Mouse.current.position.ReadValue(); // get mouse position in screen space
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane)); // convert to world position
        worldPos.z = 0f; // force z = 0 to keep in 2d
        Debug.Log(worldPos);

        return worldPos;
    }
}
