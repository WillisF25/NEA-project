using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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

    // camera use
    private List<LineRenderer> creatureLines = new List<LineRenderer>();

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

        void Update()
    {   
        // distance travelled on the x axis
        currentFitness = transform.position.x - startPosition.x;
    }

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

    public float GetFinalFitness()
    {   
        // return the fitness score at the end of the generation
        return currentFitness;
    }

    // camera logic for unrendering creatures
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