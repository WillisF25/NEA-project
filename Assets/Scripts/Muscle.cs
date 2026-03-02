using UnityEngine;

/// <summary>
/// A physics based component that acts as a lienar acuator between two rigidbodies.
/// Uses Hooke's Law with damping to simulate muscular contraction and expansion,
/// based on neural network signals.
/// </summary>
public class Muscle : MonoBehaviour
{
    public Rigidbody2D rbA;
    public Rigidbody2D rbB;

    public float minLength = 0.5f;
    public float maxLength = 1.5f;
    public float springForce = 50f; // how hard it tries to reach target length
    public float damping = 5f; // prevent inf bouncing

    private float targetLength;

    /// <summary>
    /// Sets the initial resting target length of the muscle to the mdipoint of its range.
    /// </summary>
    void Start()
    {
        // default to resting state (mid of min and max)
        targetLength = (minLength + maxLength) / 2;
    }

    /// <summary>
    /// Maps a normalised signal to a physical target length.
    /// </summary>
    /// <param name="value">A signal form -1.0 (fully contracted) to 1.0 (fully extended).</param>
    public void SetMuscleExtension(float value)
    {
        if (rbA == null || rbB == null) return;

        // map -1 to 1 output to target len between min and max
        float t = (value + 1f) / 2f;
        targetLength = Mathf.Lerp(minLength, maxLength, t);
    }

    /// <summary>
    /// Calculates and applies spring and damping forces to the connected rigidbodies.
    /// </summary>
    /// <remarks>
    /// Uses a damped harmonic oscillator formula:
    /// $F = (-k \cdot x) - (c \cdot v)$
    /// Where $k$ is the springForce, $x$ is displacement from targetLength,
    /// $c$ is damping, and $v$ is relative velocity.
    /// </remarks>
    void FixedUpdate()
    {
        if (rbA == null || rbB == null) return;

        // calc current state
        Vector2 direction = rbB.position - rbA.position;
        float currentDistance = direction.magnitude;
        Vector2 normalisedDir = direction.normalized;

        // calc relative velocity for damping
        Vector2 relativeVelocity = rbB.linearVelocity - rbA.linearVelocity;
        float velocityAlongSpring = Vector2.Dot(relativeVelocity, normalisedDir);

        // calc Hooke's Law spring force with damping force: F = -k * x - c * v
        float displacement = currentDistance - targetLength;
        float forceMagnitude = (-springForce * displacement) - (damping * velocityAlongSpring);

        // apply equal and opposite forces
        Vector2 forceVector = normalisedDir * forceMagnitude;
        rbA.AddForce(-forceVector);
        rbB.AddForce(forceVector);
    }
}