using UnityEngine;
using System.Collections.Generic;

public class NEAT : MonoBehaviour {
    public int generationNumber;
    public int populationLimit;
    public List<Creature> population;
    public List<Specie> species;
    public Innovation globalInnovationTracker;

    // NEAT parameters
    public float mutateWeightRate;
    public float addNodeRate;
    public float addConnectionRate;
    public float reenableGeneRate;
    public float crossoverRate;
    public float mutateWeightStep;
    public float compatibilityThreshold;
    public string trainingGoal;

    // code later
    public float FitnessFunction() { return 0f; }
    public void EvolvePopulaiton() {}
    public void InitialisePopulation() {}
    public void SimulateGeneration() {}
    public void EvaluateFitness() {}
    public void AdjustFitness() {}
    public void Speciate() {}
    public void Reproduce() {}
}

public class Specie {
    public int specieID;
    public List<Creature> members;
    public Creature representative;
    public float totalFitness;

    // code later
    public int DetermineOffspringCount() {return 0;}
    public List<Creature> SelectParents() {return new List<Creature>();}
}

public class Creature : MonoBehaviour {
    public int creatureID;
    public Structure structure;
    public Genome genome;

    public void Construct(int id, Structure structRef, Genome genomeRef) 
    {
        this.creatureID = id;
        this.structure = structRef;
        this.genome = genomeRef;
    }
}