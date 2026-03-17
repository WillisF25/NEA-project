using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The master NEAT algorithm handler. 
/// Handles fitness evaluation, speciation, and reproduction.
/// </summary>
[System.Serializable]
public class NEAT {
    public int generationNumber;
    public int populationLimit;
    public List<Genome> population = new List<Genome>();
    public List<Specie> species = new List<Specie>();

    // NEAT parameters
    public float mutateWeightRate;
    public float addNodeRate;
    public float addConnectionRate;
    public float reenableGeneRate;
    public float crossoverRate;
    public float mutateWeightStep;
    public float compatibilityThreshold;
    // coefficients from the NEAT paper
    public float c1; // excess
    public float c2; // disjoint
    public float c3; // weight difference
    
    // trackers
    public Innovation globalInnovationTracker;
    private int globalSpecieIDCounter = 0;
    private int globalGenomeIDCounter = 0;

    /// <summary>
    /// Sets up generation 0 with neural networks of minimal topology.
    /// </summary>
    /// <param name="jointCount">Number of sensors.</param>
    /// <param name="muscleCount">Number of output actuators.</param>
    public void InitialisePopulation(int jointCount, int muscleCount)
    {
        globalInnovationTracker = new Innovation();

        population.Clear();

        for (int i = 0; i < populationLimit; i++)
        {
            // create a starting genome (minial strucutre and random weights)
            Genome startingGenome = CreateInitialGenome(jointCount, muscleCount); 

            startingGenome.genomeID = globalGenomeIDCounter++;
            population.Add(startingGenome);
        }

        // initial speciation so every creature has a group from start
        Speciate();
    }

    /// <summary>
    /// Constructs a basec neural network with minimal connecitons for a new creature.
    /// </summary>
    /// <param name="jointCount">Sensor count.</param>
    /// <param name="muscleCount">Actuator count.</param>
    /// <returns>Simple, funcitional starting genome.</returns>
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
             float randomWeight = UnityEngine.Random.Range(-1.0f, 1.0f);

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
    
    /// <summary>
    /// Pulls performance data from simulation into the genetic data.
    /// </summary>
    /// <param name="followers">Array of creature trackers containing the displacment data of the creature.</param>
    public void EvaluateFitness(CreatureFollower[] followers)
    {
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
    
    /// <summary>
    /// Implements Fitness Sharing. Reduces the fitness of creatures that are too similar to ensure diversity.
    /// Uses the formula: $f'_i = \frac{f_i}{\sum sh(d(i,j))}$
    /// </summary>
    public void AdjustFitness()
    {
        foreach (Specie s in species)
        {
            s.totalFitness = 0; // reset total fitness for this gen

            // compare each creature (c_i) against other creatures
            foreach (Genome gi in s.members)
            {
                float sharingSum = 0;

                // calc the sharing func sum for all members in the specie
                foreach (Genome gj in s.members)
                {
                    // calc the distance between genomes
                    float dist = gi.GetCompatibilityDistance(gj, c1, c2, c3);

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
                gi.fitness = gi.fitness / sharingSum;

                // add to total for offspring calc
                s.totalFitness += gi.fitness;

            }
        }
    }

    /// <summary>
    /// Groups the populaiton into species based on topological/compatibility distance to preserve innovation.
    /// </summary>
    public void Speciate()
    {
        // clear mmebers from all species, but keep the representatives
        foreach (Specie s in species)
        {
            s.members.Clear();
        }

        // assign each creature to a species
        foreach (Genome genome in population)
        {
            bool foundSpecies = false;

            // compare creature to each exising species representative
            foreach (Specie s in species)
            {   
                // ends of species lost its representative
                // and compat dist formula
                if (s.representative != null && genome.GetCompatibilityDistance(s.representative, c1, c2, c3) < compatibilityThreshold)
                {
                    s.members.Add(genome);
                    genome.speciesID = s.specieID;
                    foundSpecies = true;
                    break;
                }
            }

            // if no compatible species, create new
            if (!foundSpecies)
            {
                Specie newSpecie = new Specie(globalSpecieIDCounter++, genome);
                genome.speciesID = newSpecie.specieID;
                species.Add(newSpecie);
            }
        }

        // remove any species with 0 creature
        species.RemoveAll(s => s.members.Count == 0);

        // update representatives for next gen
        foreach (Specie s in species)
        {
            if (s.members.Count >0) s.representative = s.members[0];
        }
    }

    /// <summary>
    /// Generates the next generation through elitism and selective breeding.
    /// </summary>
    public void Reproduce()
    {
        List<Genome> newPopulation = new List<Genome>();
        
        // calc global fitness
        float globalTotalFitness = 0;
        foreach (Specie s in species) 
        {
            globalTotalFitness += s.totalFitness;
        }

        // handle gen 0 / zero fitness edge case
        if (globalTotalFitness <= 0.001f) // basically if every creature failed
        {
            Debug.LogWarning("Global fitness was 0. Creating a completely random new generation.");    
            // mutate existing ones heavilty to force something new
            foreach (Genome g in population)
            {
                Genome mutant = g.Clone();
                mutant.Mutate(globalInnovationTracker, this);
                mutant.genomeID = globalGenomeIDCounter++;
                newPopulation.Add(mutant);            
            }
            population = newPopulation;
            return; // exit early
        }


        // normal reproduction
        // generate each specie's allotted offspring
        foreach (Specie s in species)
        {   
            // ensure species has members
            if (s.members.Count == 0) continue;

            // sort to ensure the elite is at index 0 and tournament seleciton is fast
            s.members.Sort((a, b) => b.fitness.CompareTo(a.fitness)); // highest first

            // determine how many slots this species gets in the next gen
            int offspringCount = s.DetermineOffspringCount(globalTotalFitness, populationLimit);

            if (offspringCount > 0)
            {
                // elitism: best member of the species survivies unchanged
                Genome eliteBrain = s.members[0].Clone(); // clone to ensure unique brain instance

                // instantiate the elite a fresh instance and a unique Global ID
                eliteBrain.genomeID = globalGenomeIDCounter++;
                newPopulation.Add(eliteBrain);

                // generate the remainder of the specie's slots through mating
                for (int i = 1; i < offspringCount; i++)
                {
                    Genome[] parents = s.SelectParents();
                    
                    // combine parent genomes into a new child genome.
                    Genome childGenome = parents[0].Crossover(parents[1], globalGenomeIDCounter++);
                    
                    // mutation
                    childGenome.Mutate(globalInnovationTracker, this);
                    childGenome.genomeID = globalGenomeIDCounter++;
                    newPopulation.Add(childGenome.Clone());
                }
            }
        }

        // handle rounding deficits, as FloorToInt rounds down
        // rule out species with no members
        List<Specie> activeSpecies = species.FindAll(s => s.members.Count > 0);
        if (activeSpecies.Count > 0)
        {   
            // fill the remaining slots by breeding from best specie
            activeSpecies.Sort((a, b) => b.totalFitness.CompareTo(a.totalFitness));
            Specie bestSpecie = activeSpecies[0];

            while (newPopulation.Count < populationLimit)
            {
                Genome[] parents = bestSpecie.SelectParents();
                Genome childGenome = parents[0].Crossover(parents[1], globalGenomeIDCounter++);
                childGenome.Mutate(globalInnovationTracker, this);
                childGenome.genomeID = globalGenomeIDCounter++;
                newPopulation.Add(childGenome.Clone());
            }
        }

        // overwrite old gen with new gen
        population = newPopulation;
    }
}

/// <summary>
/// A tracker that assigns unique Ids to structural changes in the neural netowork for historical tracking.
/// </summary>
public class Innovation {
    // increments when a unique mutation occurs
    private int innovationTracker = 0;
    
    // key: InnovationID
    // value: [string mutationType, int nodeInID, int nodeOutID]
    // or for nodes: [string mutationType, int oldConnectionID]
    private Dictionary<int, List<object>> innovationRecords = new Dictionary<int, List<object>>();

    /// <summary>
    /// Gets a unique ID for a connection mutation,
    ///  or returns an existing one of this mutation has occured before.
    /// </summary>
    /// <param name="nodeInID">Source node ID.</param>
    /// <param name="nodeOutID">Target node ID.</param>
    /// <param name="mutationType">Type of mutation occuring.</param>
    /// <returns>A unique innovation ID.</returns>
    public int GetInnovationID(int nodeInID, int nodeOutID, string mutationType) 
    {
        // loop through existing records
        foreach (var record in innovationRecords)
        {
            string type = (string)record.Value[0];
            int inID = (int)record.Value[1];
            int outID = (int)record.Value[2];

            if (type == mutationType && inID == nodeInID && outID == nodeOutID)
            {
                return record.Key; // return exsiting id
            }
        }

        // brand new muation if got here
        int newID = innovationTracker;

        // record it
        List<object> newRecord = new List<object> { mutationType, nodeInID, nodeOutID };
        innovationRecords.Add(newID, newRecord);

        // increment global tracker for next unique mutation
        innovationTracker++;

        return newID;
    }

    /// <summary>
    /// Gets a unique ID for a node created by splitting a connection.
    /// </summary>
    /// <param name="connectionID">The ID or the connection being split.</param>
    /// <returns>A unique node innovation ID.</returns>
    public int GetNodeInnovationNumber(int connectionID)
    {
        string type = "AddNode";
        foreach (var record in innovationRecords)
        {
            if ((string)record.Value[0] == type && (int)record.Value[1] == connectionID)
            {
                return record.Key;
            }
        }

        int newID = innovationTracker;
        innovationRecords.Add(newID, new List<object> { type, connectionID, -1 }); // -1 as placeholder
        innovationTracker++;
        return newID;
    }
}

/// <summary>
/// Represents a group of similar genomes.
/// Used to protect new innovations from being out competed by well established structures.
/// </summary>
[System.Serializable]
public class Specie {
    public int specieID;
    public List<Genome> members = new List<Genome>();
    public Genome representative;
    public float totalFitness;

    public Specie(int id, Genome firstMember)
    {
        specieID = id;
        representative = firstMember;
        members.Add(firstMember);
    }

    /// <summary>
    /// Resets the specie data for a new generation,
    /// while keeping the representative.
    /// </summary>
    public void Reset()
    {
        members.Clear();
        totalFitness = 0;
    }

    /// <summary>
    /// Calculates how many offsprint this species is allowd based on its proportion it the total global fitness.
    /// </summary>
    /// <param name="globalTotalFitness">Sum of all adjusedt fitness in the population.</param>
    /// <param name="popLimit">Maximum population size.</param>
    /// <returns>The number of slots in the next generation.</returns>
    public int DetermineOffspringCount(float globalTotalFitness, int popLimit)
    {
        if (globalTotalFitness == 0)
        {
            return popLimit; // handled in Reproduce to split evenly
        }

        float percentage = this.totalFitness / globalTotalFitness;

        return Mathf.FloorToInt(percentage * popLimit);
    }

    /// <summary>
    /// Pickes two parents from the top half of the species,
    /// using Tournament selection.
    /// </summary>
    /// <returns>An array of two parent genomes.</returns>
    public Genome[] SelectParents()
    {
        // assume members list is already sorted by fitness (highest to lowest)

        // truncation: conly consider top 50%
        int eligibleCount = Mathf.Max(1, Mathf.CeilToInt(members.Count / 2f));
        Genome[] parents = new Genome[2];

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
