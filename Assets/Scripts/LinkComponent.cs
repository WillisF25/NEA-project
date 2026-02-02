using UnityEngine;

public class LinkComponent : MonoBehaviour
{   
    public Rigidbody2D jointA;
    public Rigidbody2D jointB;
    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (jointA != null && jointB != null)
        {
            // update the line renderer pos to match the moving joints
            lr.SetPosition(0, jointA.position);
            lr.SetPosition(1, jointB.position);
        }
    }
}
