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
    public float frequency = 1.0f; // how many pluse per second

    void Start()
    {
        if (joint == null)
            joint = GetComponent<DistanceJoint2D>();
    }

    void Update()
    {
        if (joint == null) return;

        // calc the sine wave
        float phase = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI);
        
        // map -1 to 1 sine wave range to from 0 to 1
        float normalisedWave = (phase + 1f) / 2f;

        // interpolate the dist between min and max
        joint.distance = Mathf.Lerp(minLength, maxLength, normalisedWave);
    }
}