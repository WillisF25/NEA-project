using UnityEngine;
using System.Collections.Generic;

public class NEAT : MonoBehaviour {
    public int generationNumber;
    public int populationLimit;
    public List<Creature> population = new List<Creature>();
    public List<Specie> species = new List<Specie>();

    // NEAT parameters
    public float mutateWeightRate;
    public float addNodeRate;
    public float addConnectionRate;
    public float reenableGeneRate;
    public float crossoverRate;
    public float mutateWeightStep;
    public string trainingGoal;
    public float compatibilityThreshold;
    // coefficients from the NEAT paper
    public float c1 = 1.0f; // excess
    public float c2 = 1.0f; // disjoint
    public float c3 = 0.4f; // weight difference
    
    // trackers
    public Innovation globalInnovationTracker;
    private int globalSpecieIDCounter = 0;
    private int globalCreatureIDCounter = 0;
    private int globalGenomeIDCounter = 0;

    // code later
    public float FitnessFunction() { return 0f; }
    public void EvolvePopulaiton() {}

    // called once to generate generation 0
    public void InitialisePopulation(int jointCount, int muscleCount)
    {
        globalInnovationTracker = new Innovation();

        population.Clear();

        for (int i = 0; i < populationLimit; i++)
        {
            // create a starting genome (minial strucutre and random weights)
            Genome startingGenome = CreateInitialGenome(jointCount, muscleCount); 

            Creature newCreature = new Creature(globalCreatureIDCounter++, new Structure(), startingGenome);
            population.Add(newCreature);
        }

        // initial speciation so every creature has a group from start
        Speciate();
    }

    private Genome CreateInitialGenome(int jointCount, int muscleCount)
    {
        Genome g = new Genome(globalGenomeIDCounter++);
        int nodeID = 0;

        // create 1 input per joint and plus one for oscillator
        int oscillatorIndex = 0;
        int inputCount = 1 + jointCount;
        for (int i = 0; i < inputCount; i++)
            g.nodes.Add(new NodeGene(nodeID++, "INPUT", "Linear", 0));

        // create 1 output per muscle
        int firstOutputIndex = nodeID;
        for (int i = 0; i < muscleCount; i++)
            g.nodes.Add(new NodeGene(nodeID++, "OUTPUT", "Tanh", 0));

        // connects the oscillator to every muscle
        for (int i = 0; i < muscleCount; i++)
        {
            // connect node 0 (oscillator) to node (firstOutputIndex + i)
            // randome weight between -1 and 1 for variety
             float randomWeight = Random.Range(-1.0f, 1.0f);

            int innovationID = globalInnovationTracker.GetInnovationID(
            oscillatorIndex,
            firstOutputIndex + i,
            "AddConnection"
            );

             g.connections.Add(new ConnectionGene(
                innovationID,
                oscillatorIndex,
                firstOutputIndex + i,
                randomWeight,
                true
                ));
          }

        return g;
    }
        
    public void SimulateGeneration() {}
    public void EvaluateFitness()
    {
        // find every follower scirpt
        CreatureFollower[] followers = FindObjectsByType<CreatureFollower>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        // map performce back to genetic data
        foreach (CreatureFollower follower in followers)
        {
            if (follower.assignedGenome != null)
            {
                // get displacement
                float performance = follower.GetFinalFitness();

                // this prevents negative fitness from breaking the NEAT sharing math.
                follower.assignedGenome.fitness = Mathf.Max(0.001f, performance);
            }
        }
    }
    
    public void AdjustFitness()
    {
        foreach (Specie s in species)
        {
            s.totalFitness = 0; // reset total fitness for this gen

            // compare each creature (c_i) against other creatures
            foreach (Creature ci in s.members)
            {
                float sharingSum = 0;

                // calc the sharing func sum for all members in the specie
                foreach (Creature cj in s.members)
                {
                    // calc the distance between genomes
                    float dist = ci.genome.GetCompatibilityDistance(cj.genome, c1, c2, c3);

                    // they share fitness if within the threshold
                    if (dist < compatibilityThreshold)
                    {
                        // continous sharing func: 1- (dist/threshodl)^2
                        sharingSum += 1.0f - Mathf.Pow(dist / compatibilityThreshold, 2);
                    }
                }

                // against division by zero, sum will be at least 1
                if (sharingSum < 1.0f) sharingSum = 1.0f;

                // adjusted fitness formula: f'i = fi / sum(sh(dist))
                ci.genome.fitness = ci.genome.fitness / sharingSum;

                // add to total for offspring calc
                s.totalFitness += ci.genome.fitness;

            }
        }
    }

    public void Speciate()
    {
        // clear mmebers from all species, but keep the representatives
        foreach (Specie s in species)
        {
            s.members.Clear();
        }

        // assign each creature to a species
        foreach (Creature creature in population)
        {
            bool foundSpecies = false;

            // compare creature to each exising species representative
            foreach (Specie s in species)
            {
                // compat dist formula
                if (creature.genome.GetCompatibilityDistance(s.representative.genome, c1, c2, c3) < compatibilityThreshold)
                {
                    s.members.Add(creature);
                    foundSpecies = true;
                    break;
                }
            }

            // if no compatible species, create new
            if (!foundSpecies)
            {
                Specie newSpecie = new Specie(globalSpecieIDCounter++, creature);
                species.Add(newSpecie);
            }

            // remove any species with 0 creature
            species.RemoveAll(s => s.members.Count == 0);

        }
    }

    public void Reproduce()
    {
        List<Creature> newPopulation = new List<Creature>();

        float globalTotalFitness = 0;
        foreach (Specie s in species) 
        {
            globalTotalFitness += s.totalFitness;
        }

        // generate each specie's allotted offspring
        foreach (Specie s in species)
        {   
            // sort to ensure the elite is at index 0 and tournament seleciton is fast
            s.members.Sort((a, b) => b.genome.fitness.CompareTo(a.genome.fitness));

            // determine how many slots this speceis gets in the next gen
            int offspringCount = s.DetermineOffspringCount(globalTotalFitness, populationLimit);

            if (offspringCount > 0)
            {
                // elitism: best member of the speceis survivies unchanged
                Genome eliteBrain = s.members[0].genome.Clone(); // clone to ensure unique brain instance

                // instantiate the elite a fresh instance and a unique Global ID
                newPopulation.Add(new Creature(globalCreatureIDCounter++, new Structure(), eliteBrain));

                // generate the remainder of the specie's slots through mating
                for (int i = 1; i < offspringCount; i++)
                {
                    Creature[] parents = s.SelectParents();
                    
                    // combine parent genomes into a new child genome.
                    Genome childGenome = parents[0].genome.Crossover(parents[1].genome);
                    
                    // mutation
                    childGenome.Mutate(globalInnovationTracker);

                    newPopulation.Add(new Creature(globalCreatureIDCounter++, new Structure(), childGenome));
                }
            }
        }

        // handle rounding deficits, as FloorToInt rounds down
        // fill the remaining slot by breeding from top performing species
        while (newPopulation.Count < populationLimit)
        {
            // sort species by total fitness to find the fittest species
            species.Sort((a, b) => b.totalFitness.CompareTo(a.totalFitness));
            Specie bestSpecie = species[0];
            
            Creature[] parents = bestSpecie.SelectParents();
            Genome childGenome = parents[0].genome.Crossover(parents[1].genome);
            childGenome.Mutate(globalInnovationTracker);
            
            newPopulation.Add(new Creature(globalCreatureIDCounter++, new Structure(), childGenome));
        }

        // overwrite old gen with new gen
        population = newPopulation;
    }
}

public class Specie {
    public int specieID;
    public List<Creature> members = new List<Creature>();
    public Creature representative;
    public float totalFitness;

    public Specie(int id, Creature firstMember)
    {
        this.specieID = id;
        this.representative = firstMember;
        this.members.Add(firstMember);
    }

    public void Reset()
    {
        members.Clear();
        totalFitness = 0;
    }

    public int DetermineOffspringCount(float globalTotalFitness, int popLimit)
    {
        if (globalTotalFitness == 0)
        {
            return popLimit; // handled in Reproduce to split evenly
        }

        float percentage = this.totalFitness / globalTotalFitness;

        return Mathf.FloorToInt(percentage * popLimit);
    }

    public Creature[] SelectParents()
    {
        // assume members list is already sorted by fitness (highest to lowest)

        // truncation: conly consider top 50%
        int eligibleCount = Mathf.Max(1, members.Count / 2);
        Creature[] parents = new Creature[2];

        for (int i = 0; i < 2; i++)
        {
            // tournament selection
            int idA = UnityEngine.Random.Range(0, eligibleCount);
            int idB = UnityEngine.Random.Range(0, eligibleCount);

            // since list is sorted, lower index is fitter
            parents[i] = idA < idB ? members[idA] : members[idB];
        }

        return parents;
    }
}

public class Creature : MonoBehaviour {
    public int creatureID;
    public Structure structure;
    public Genome genome;
    public float fitness;

    public Creature(int id, Structure structRef, Genome genomeRef) 
    {
        this.creatureID = id;
        this.structure = structRef;
        this.genome = genomeRef;
        this.fitness = 0f;
    }
}