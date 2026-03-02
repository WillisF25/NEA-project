using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The controller for an individual creature instance.
/// Acts as the interface between the Neural Network and the physics components (brain and body).
/// Tracks fitness and handles visual rendering states.
/// </summary>
public class CreatureFollower : MonoBehaviour
{
    private NeuralNetwork network;
    private List<Muscle> muscles = new List<Muscle>();
    private List<Transform> joints = new List<Transform>();

    // pre allocated buffers
    private float[] inputs;
    private float[] outputs;
    
    // fitness variables
    public Genome assignedGenome;
    private Vector3 startPosition;
    public float currentFitness;
    private float maxDistance = -Mathf.Infinity;
    public Transform leadingJoint;

    // camera use
    private List<LineRenderer> creatureLines = new List<LineRenderer>();

    /// <summary>
    /// Initialises the creature's brain and links it to its physical parts.
    /// </summary>
    /// <param name="genome">The gentic blueprint for the neural network.</param>
    /// <param name="creatureMuscles">List of Muscle components to be controlled.</param>
    /// <param name="creatureJoints">List of Joint transforms used as sensors.</param>
    /// <param name="lines">List of LineRenderers for visual fading logic.</param>
    public void Init(Genome genome, List<Muscle> creatureMuscles, List<Transform> creatureJoints, List<LineRenderer> lines)
    {
        network = new NeuralNetwork(genome);
        muscles = creatureMuscles;
        joints = creatureJoints;
        assignedGenome = genome;
        startPosition = transform.position;

        // initialise buffer
        inputs = new float[1 + joints.Count]; // oscillator + joint heights

        creatureLines = lines;
    }

    /// <summary>
    /// Updates the fitness score every frame based on the furthest distance reached.
    /// </summary>
    void Update()
    {   
        UpdateMaxDistance();
        currentFitness = maxDistance;
    }

    /// <summary>
    /// Scans all joints to find which one is furthest along the x-axis.
    /// Updates the record holding joint for camera tracking.
    /// </summary>
    public void UpdateMaxDistance()
    {
                foreach (Transform j in joints)
        {
            float jPosX = j.position.x;
            if (jPosX > maxDistance)
            {
                maxDistance = jPosX;
                leadingJoint = j;
            }
        }
    }

    /// <summary>
    /// Handles the "thinking" process. Gathers environment data, runs it through 
    /// the network, and then applies the resulting forces to the muscles.
    /// </summary>
    /// <remarks>
    /// Inputs: 
    /// 0: Global Sine Oscillator (Time-based rhythmic input).
    /// 1 to n: Local y-position (height) of each joint.
    /// </remarks>
    void FixedUpdate() // FixedUpdate for physcis consistency
    {
        if (network == null) return;

        // input A, Oscillator
        // helps with rhythimc movement
        inputs[0] = Mathf.Sin(Time.time * 2f);

        // input B, joint posisiton (height)
        // helps the creature know where it is relative to the ground
        for(int i = 0; i < joints.Count; i++) 
        {
            inputs[1 + i] = joints[i].localPosition.y;
        }

        // process through the neural network
        outputs = network.ForwardPass(inputs);

        // apply ouputs to Muscles
        if (outputs != null && outputs.Length == muscles.Count)
        {
            for (int i = 0; i < muscles.Count; i++)
            {
                muscles[i].SetMuscleExtension(outputs[i]);
            }
        }
    }

    /// <summary>
    /// Returns the highest x-coordinate reached by this creature during its lifespan.
    /// </summary>
    /// <returns>A float representing the fitness score.</returns>
    public float GetFinalFitness()
    {   
        // return the fitness score at the end of the generation
        return currentFitness;
    }

    /// <summary>
    /// Adjusts the transparency of the creature's sprites and lines.
    /// Used by the SimulationManager to focus on the best creature.
    /// </summary>
    /// <param name="alpha">The transparency level (0.0 to 1.0).</param>
    public void SetVisibility (float alpha)
    {   
        // update Joints
        foreach (Transform t in joints)
        {
            if (t == null) continue;
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        // update Links
        foreach (LineRenderer lr in creatureLines)
        {   
            if (lr == null) continue;

            // only update the alpha of existing start/end
            Color start = lr.startColor;
            start.a = alpha;
            lr.startColor = start;

            Color end = lr.endColor;
            end.a = alpha;
            lr.endColor = end;
        }
    }
}