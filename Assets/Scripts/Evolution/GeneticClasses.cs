using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Container for a creature's genetic information.
/// Acts as the blueprint for the Neural Network topology.
/// </summary>
[System.Serializable]
public class Genome {
    public int genomeID;
    public int speciesID = -1;
    public List<ConnectionGene> connections = new List<ConnectionGene>();
    public List<NodeGene> nodes = new List<NodeGene>();
    public float fitness;

    public Genome(int genomeID) 
    {
        this.genomeID = genomeID;
    }

    /// <summary>
    /// Master mutation funciton that decides for structural or weight changes.
    /// </summary>
    /// <param name="innovation">The global tracker for innovation.</param>
    /// <param name="settings">The NEAT settings continaing mutation weights and steps.</param>
    public void Mutate(Innovation innovation, NEAT settings)
    {
        // mutate weights
        if (UnityEngine.Random.value < settings.mutateWeightRate) 
        {
            MutateWeights(settings.mutateWeightStep);
        }

        // add Connection 
        if (UnityEngine.Random.value < settings.addConnectionRate) 
        {
            AddConnection(innovation);
        }

        // add Node
        if (UnityEngine.Random.value < settings.addNodeRate) 
        {
            AddNode(innovation);
        }
    }

    /// <summary>
    /// Iterates through connection weights and applies either a small nudge (perturb) or a total reset.
    /// </summary>
    /// <param name="step">The maximum range for weight perturbation.</param>
    public void MutateWeights(float step)
    {
        foreach (ConnectionGene conn in connections)
        {
            if (UnityEngine.Random.value < 0.9f) // 90% chance to preturb
            {
                // preturb existing weight slightly
                conn.weight += UnityEngine.Random.Range(-step, step);
            }
            else if (UnityEngine.Random.value < 0.05f) // small chance for replace
            {
                conn.weight = UnityEngine.Random.Range(-2f, 2f);
            }
        }
    }
    
    /// <summary>
    /// Adds a new connection between two nodes.
    /// </summary>
    /// <param name="innovation">The global tracker for innovation.</param>
    public void AddConnection(Innovation innovation)
    {
        // separate nodes inot potential sources and potential targets
        // input nodes can only be sources, output nodes canonly be targets
        // hiddne nodes can be both
        List<NodeGene> potentialSources = new List<NodeGene>();
        List<NodeGene> potentialTargets = new List<NodeGene>();

        foreach (var node in nodes)
        {
            if (node.nodeType != "OUTPUT") potentialSources.Add(node);
            if (node.nodeType != "INPUT") potentialTargets.Add(node);
        }

        // pick randomly to find an unconnected pair
        int attempts = 0;
        while (attempts < 30) // avoid infinite loops
        {
            attempts++;

            NodeGene source = potentialSources[UnityEngine.Random.Range(0, potentialSources.Count)];
            NodeGene target = potentialTargets[UnityEngine.Random.Range(0, potentialTargets.Count)];

            // no self connections
            if (source.innovationID == target.innovationID) continue;

            // check if conneciton already exists
            if (ConnectionExists(source.innovationID, target.innovationID)) continue;

            // create the connection
            int id = innovation.GetInnovationID(source.innovationID, target.innovationID, "AddConnection");
            connections.Add(new ConnectionGene(id, source.innovationID, target.innovationID, UnityEngine.Random.Range(-1f, 1f), true));
            return;
        }
    }
    /// <summary>
    /// Checks if a conneciton already exists between two specific nodes to avoid duplicate connections.
    /// </summary>
    /// <param name="node1">Innnovation ID of source node.</param>
    /// <param name="node2">Innovation ID of target node.</param>
    /// <returns></returns>
    private bool ConnectionExists(int node1, int node2)
    {
        foreach (var conn in connections)
        {
            if (conn.inputNode == node1 && conn.outputNode == node2) return true;
        }
        return false;
    }

    /// <summary>
    /// Splits a connection to insert a new hidden node.
    /// Allowing the network to grow in complexity.
    /// </summary>
    /// <param name="innovation">The global tracker for Innovation ID</param>
    public void AddNode(Innovation innovation)
    {
        // filter for only enabled connections
        List<ConnectionGene> enabledConnections = connections.FindAll(c => c.enabled);
        if (enabledConnections.Count == 0) return;

        // select a randome connection to split
        ConnectionGene connToSplit = enabledConnections[UnityEngine.Random.Range(0, enabledConnections.Count)];

        // disable the old one
        connToSplit.enabled = false;

        // get the ID for new node
        int newNodeID = innovation.GetNodeInnovationNumber(connToSplit.innovationID); 

        // only add node if genome doesnt have it
        if (nodes.Find(n => n.innovationID == newNodeID) == null) 
        {
            NodeGene newNode = new NodeGene(newNodeID, "HIDDEN", "Sigmoid", 0f);
            nodes.Add(newNode);
        }

        // link 1: source to newnode (weight = 1)
        int link1_ID = innovation.GetInnovationID(connToSplit.inputNode, newNodeID, "AddConnection");
        ConnectionGene link1 = new ConnectionGene(link1_ID, connToSplit.inputNode, newNodeID, 1.0f, true);

        // link2: newnode to target (weight = old weight)
        int link2_ID = innovation.GetInnovationID(newNodeID, connToSplit.outputNode, "AddConnection");
        ConnectionGene link2 = new ConnectionGene(link2_ID, newNodeID, connToSplit.outputNode, connToSplit.weight, true);

        connections.Add(link1);
        connections.Add(link2);
    }

    /// <summary>
    /// Sexual reproduction logic. Inherits mathcing genes from both parent,
    /// and non mathcing genes form the fitter parent.
    /// </summary>
    /// <param name="partner">The other genome to mate with.</param>
    /// <param name="newID">ID of the resulting offspring.</param>
    /// <returns>The Genome of the resulting offspring.</returns>
    public Genome Crossover(Genome partner, int newID) // pass in unique id
    {
        // determine relative fitness
        bool equalFitness = Mathf.Approximately(fitness, partner.fitness);
        Genome fitter = fitness > partner.fitness ? this : partner;
        Genome loser = fitness > partner.fitness ? partner : this;

        Genome child = new Genome(newID);

        // dict for matching genes
        Dictionary<int, ConnectionGene> loserGenes = new Dictionary<int, ConnectionGene>();
        foreach (var gene in loser.connections) 
        {
            loserGenes[gene.innovationID] = gene; 
        }

        // inherit connections
        foreach (ConnectionGene fitterGene in fitter.connections)
        {
            if (loserGenes.ContainsKey(fitterGene.innovationID))
            {
                // matching: pick randomly form either parent
                ConnectionGene loserGene = loserGenes[fitterGene.innovationID];
                float selectedWeight = UnityEngine.Random.value < 0.5f ? fitterGene.weight : loserGene.weight;
                
                // if either parent is disabled, child as a high chance (75%) of being disabled
                bool isEnabled = true;
                if ((!fitterGene.enabled || !loserGene.enabled) && UnityEngine.Random.value < 0.75f)
                    isEnabled = false;

                child.connections.Add(new ConnectionGene(fitterGene.innovationID, fitterGene.inputNode, fitterGene.outputNode, selectedWeight, isEnabled));
            }
            else
            {
                // disjoint/excess: inherit from fitter
                child.connections.Add(new ConnectionGene(fitterGene.innovationID, fitterGene.inputNode, fitterGene.outputNode, fitterGene.weight, fitterGene.enabled));
            }
        }

        // equal fitness
        if (equalFitness)
        {   
            // inherit disjoint/excess genes from both to maximise diversity
            foreach (ConnectionGene loserGene in loser.connections)
            {
                if (child.connections.Find(c => c.innovationID == loserGene.innovationID) == null)
                {
                    child.connections.Add(new ConnectionGene(loserGene.innovationID, loserGene.inputNode, loserGene.outputNode, loserGene.weight, loserGene.enabled));
                }
            }
        }

        // node inheritance
        HashSet<int> addedNodeIDs = new HashSet<int>();

        // inherites all nodes form fitter, to ensure potential for future mutations
        foreach (var node in fitter.nodes)
        {
            child.nodes.Add(new NodeGene(node.innovationID, node.nodeType, node.activation, node.bias));
            addedNodeIDs.Add(node.innovationID);
        }

        // if equal fitness, also add nodes form loser that are unique
        if (equalFitness)
        {
            foreach (var node in loser.nodes)
            {
                // rule out existing nodes
                if (!addedNodeIDs.Contains(node.innovationID))
                {
                    child.nodes.Add(new NodeGene(node.innovationID, node.nodeType, node.activation, node.bias));
                    addedNodeIDs.Add(node.innovationID);
                }
            }

        }

        return child;
    }

    /// <summary>
    /// Calculates a rating of how similar two networks/genomes are.
    /// Used to determine if they are in the same specie.
    /// </summary>
    /// <param name="partner">The genome to compare against.</param>
    /// <param name="c1">Excess gene coeffcient.</param>
    /// <param name="c2">Disjoint gene coeffcient.</param>
    /// <param name="c3">Weight difference coeffcient.</param>
    /// <returns>Float value representing topological and weight difference.</returns>
    public float GetCompatibilityDistance(Genome partner, float c1, float c2, float c3)
    {
        int matching = 0;
        int disjoint = 0;
        int excess = 0;
        float weightDiffSum = 0;

        // sort both connection lists by Innovation ID
        List<ConnectionGene> genes1 = new List<ConnectionGene>(this.connections);
        List<ConnectionGene> genes2 = new List<ConnectionGene>(partner.connections);
        genes1.Sort((a, b) => a.innovationID.CompareTo(b.innovationID));
        genes2.Sort((a, b) => a.innovationID.CompareTo(b.innovationID));

        int i = 0;
        int j = 0;

        // parallel iteration
        while (i < genes1.Count && j < genes2.Count)
        {
            ConnectionGene g1 = genes1[i];
            ConnectionGene g2 = genes2[j];

            if (g1.innovationID == g2.innovationID)
            {
                matching++;
                weightDiffSum += Mathf.Abs(g1.weight - g2.weight);
                i++;
                j++;
            }
            else if (g1.innovationID < g2.innovationID)
            {
                disjoint++;
                i++;
            }
            else
            {
                disjoint++;
                j++;
            }
        }

        // calculate Excess (the genes left over at the end of the longer list)
        excess = (genes1.Count - i) + (genes2.Count - j);

        // normalization factor N (the size of the larger genome)
        float n = Mathf.Max(genes1.Count, genes2.Count);
        if (n < 1) n = 1; // Prevent division by zero

        float avgWeightDiff = matching > 0 ? weightDiffSum / matching : 0;

        // formula from neat paper
        return (c1 * excess / n) + (c2 * disjoint / n) + (c3 * avgWeightDiff);
    }

    /// <summary>
    /// Creates a copy of the genome to ensure mutation of a child does not affect the parent.
    /// </summary>
    /// <returns>The cloned Genome.</returns>
    public Genome Clone()
    {
        Genome clone = new Genome(this.genomeID);
        foreach (NodeGene n in nodes) clone.nodes.Add(new NodeGene(n.innovationID, n.nodeType, n.activation, n.bias));
        foreach (ConnectionGene c in connections) clone.connections.Add(new ConnectionGene(c.innovationID, c.inputNode, c.outputNode, c.weight, c.enabled));
        return clone;
    }
}

/// <summary>
/// Genetic data representing a node in the neural network.
/// </summary>
[System.Serializable]
public class NodeGene {
    public int innovationID;
    public string nodeType;
    public string activation;
    public float bias;

    public NodeGene(int innovationID, string nodeType, string activation, float bias) 
    {
        this.innovationID = innovationID;
        this.nodeType = nodeType;
        this.activation = activation;
        this.bias = bias;
    }
}

/// <summary>
/// Genetic data representing a connection in the neural network.
/// </summary>
[System.Serializable]
public class ConnectionGene {
    public int innovationID;
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool enabled;

    public ConnectionGene(int innovationID, int inputNode, int outputNode, float weight, bool enabled) 
    {
        this.innovationID = innovationID;
        this.inputNode = inputNode;
        this.outputNode = outputNode;
        this.weight = weight;
        this.enabled = enabled;
    }
}