using UnityEngine;

public class PhysicalInfiniteFloor : MonoBehaviour
{
    private float width;
    private Transform camTransform;

    void Start()
    {
        // get the width of the floor
        width = GetComponent<BoxCollider2D>().size.x * transform.localScale.x;
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        // if the camera has moved past the center the floor
        if (camTransform.position.x > transform.position.x + width)
        {
            // move the floor forward by double its width
            Vector3 newPos = transform.position;
            newPos.x += width * 2; 
            transform.position = newPos;
        }
    }
}