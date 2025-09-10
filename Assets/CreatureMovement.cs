using UnityEngine;

public class CreatureMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 2f;
    private Rigidbody2D rb;
    private Vector2 movementDirection;
    void Start()
    {
        // Get the Rigidbody2D attached to this object
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movementDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    void FixedUPdate()
    {
        rb.linearVelocity = movementDirection * movementSpeed;
    }
}
