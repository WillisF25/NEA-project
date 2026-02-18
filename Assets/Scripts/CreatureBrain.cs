using UnityEngine;
using System.Collections.Generic;

public class CreatureBrain : MonoBehaviour
{
    private NeuralNetwork network;
    private List<Muscle> muscles = new List<Muscle>();
    private List<Transform> joints = new List<Transform>();

    // init the brain with a genome
    public void Init(Genome genome, List<Muscle> creatureMuscles, List<Transform> creatureJoints)
    {
        this.network = new NeuralNetwork(genome);
        this.muscles = creatureMuscles;
        this.joints = creatureJoints;
    }

    void FixedUpdate() // FixedUpdate for physcis consistency
    {
        if (network == null) return;

        // gather inputs
        List<float> inputs = new List<float>();

        // input A, Oscillator
        // helps with rhythimc movement
        inputs.Add(Mathf.Sin(Time.time * 2f));

        // input B, joint posisiton (height)
        // helps the creature know where it is relative to the ground
        foreach (var joint in joints)
        {
            inputs.Add(joint.localPosition.y); 
        }

        // process through the neural network
        List<float> outputs = network.ForwardPass(inputs);

        // apply ouputs to Muscles
        if (outputs != null && outputs.Count == muscles.Count)
        {
            for (int i = 0; i < muscles.Count; i++)
            {
                muscles[i].SetMuscleExtension(outputs[i]);
            }
        }
    }
}