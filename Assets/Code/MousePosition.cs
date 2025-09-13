using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MousePosition : MonoBehaviour
{
    public GameObject square;

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue(); // get mouse position in screen space
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane)); // convert to world position
            worldPos.z = 0f; // force z = 0 to keep in 2d
            Debug.Log("x: " + mousePos.x + " y: " + mousePos.x);
            Debug.Log("WorldPos: " + worldPos);
            Instantiate(square, worldPos, Quaternion.identity); // generate the square based on where the mouse left clicked
        }
    }
}
