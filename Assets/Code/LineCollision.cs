using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineController))]
public class LineCollision : MonoBehaviour
{
    LineController lc;
    // the points to draw a collision shape between
    List<Vector2> colliderPoints = new List<Vector2>();
    void Start()
    {
        lc = GetComponent<LineController>();
    }

    void Update()
    {

    }
}
