using UnityEngine;

public class Muscle : MonoBehaviour
{
    // the joint this muscle controls
    public DistanceJoint2D joint;

    // default vals for now
    public float minLength = 0.5f;
    public float maxLength = 1.5f;
    public float strength = 5.0f;
    public float damping = 0.5f;

    // a frequency of expand/contract for now
    public float frequency = 1.0f; 

    void Start()
    {
        if (joint == null)
            joint = GetComponent<DistanceJoint2D>();
            
        // later code for init
    }
    
    // laster code for expand/constract in Update()
}