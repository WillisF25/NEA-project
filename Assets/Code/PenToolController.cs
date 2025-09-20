using UnityEngine;
using UnityEngine.InputSystem;

public class PenToolController : MonoBehaviour
{
    [Header("Dots")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] Transform dotParent;

    [Header("Lines")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] Transform lineParent;
    [SerializeField] private MouseManager mousem;
    private LineController currentLine;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) //left click
        {
            if (currentLine == null)
            {
                currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, lineParent).GetComponent<LineController>();
            }

            GameObject dot = Instantiate(dotPrefab, mousem.GetMousePosition(), Quaternion.identity, dotParent);
            currentLine.AddPoint(dot.transform);
        }
    }
}

