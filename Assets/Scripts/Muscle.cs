using UnityEngine;

public class Muscle : MonoBehaviour
{
    // the joint this muscle controls
    public DistanceJoint2D joint;

    // default vals for now
    public float minLength = 0.5f;
    public float maxLength = 1.5f;
    public float strength = 5.0f; // expand/contract speed

    void Start()
    {
        if (joint == null) joint = GetComponent<DistanceJoint2D>();

        // tell joint to not manage its own distance
        joint.autoConfigureConnectedAnchor = false;
    }

    public void SetMuscleExtension(float value)
    {
        if (joint == null) return;
        // map (-1 to 1) nn output range to from 0 to 1
        float t = (value + 1f) / 2f; 

        // interpolate the dist between min and max
        float targetdistance = Mathf.Lerp(minLength, maxLength, t);

        // friction fix
        joint.distance = Mathf.MoveTowards(joint.distance, targetdistance, strength);
    }
}