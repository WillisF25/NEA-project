using UnityEngine;
using System;
using System.Collections.Generic;
using Mono.Cecil;

[Serializable]
public class Genome {
    public int genomeID;
    public List<ConnectionGene> connections = new List<ConnectionGene>();
    public List<NodeGene> nodes = new List<NodeGene>();
    public float fitness;

    public Genome(int genomeID) 
    {
        this.genomeID = genomeID;
    }

    // default values for now
    public void Mutate(Innovation innovation)
    {
        // mutate weights
        if (UnityEngine.Random.value < 0.8f) 
        {
            MutateWeights();
        }

        // add Connection 
        if (UnityEngine.Random.value < 0.05f) 
        {
            AddConnection(innovation);
        }

        // add Node
        if (UnityEngine.Random.value < 0.01f) 
        {
            AddNode(innovation);
        }
    }
    public void MutateWeights()
    {
        foreach (ConnectionGene conn in connections)
        {
            if (UnityEngine.Random.value < 0.5f)
            {
                // preturb existing weight slightly
                conn.weight += UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            else if (UnityEngine.Random.value < 0.05f) // small chance for replace
            {
                conn.weight = UnityEngine.Random.Range(-2f, 2f);
            }
        }
    }
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
            int id = innovation.GetInnovationNumber(source.innovationID, target.innovationID, "AddConnection");
            connections.Add(new ConnectionGene(id, source.innovationID, target.innovationID, UnityEngine.Random.Range(-1f, 1f), true));
            return;
        }
    }
    private bool ConnectionExists(int node1, int node2)
    {
        foreach (var conn in connections)
        {
            if (conn.inputNode == node1 && conn.outputNode == node2) return true;
        }
        return false;
    }

    public void AddNode(Innovation innovation)
    {
        // filter for only enabled connections
        List<ConnectionGene> enabledConnections = connections.FindAll(c => c.enabled);
        if (enabledConnections.Count == 0) return;

        // select a randome connection to split
        ConnectionGene connToSplit = enabledConnections[UnityEngine.Random.Range(0, enabledConnections.Count)];

        // disable the old one
        connToSplit.enabled = false;

        // create new node
        // pass id of the conn to split so tracker can check if this split has happened before
        int newNodeID = innovation.GetNodeInnovationNumber(connToSplit.innovationID);    
        NodeGene newNode = new NodeGene(newNodeID, "HIDDEN", "Sigmoid", 0f);
        nodes.Add(newNode);

        // link 1: source to newnode (weight = 1)
        int link1_ID = innovation.GetInnovationNumber(connToSplit.inputNode, newNodeID, "AddConnection");
        ConnectionGene link1 = new ConnectionGene(link1_ID, connToSplit.inputNode, newNodeID, 1.0f, true);

        // link2: newnode to target (weight = old weight)
        int link2_ID = innovation.GetInnovationNumber(newNodeID, connToSplit.outputNode, "AddConnection");
        ConnectionGene link2 = new ConnectionGene(link2_ID, newNodeID, connToSplit.outputNode, connToSplit.weight, true);

        connections.Add(link1);
        connections.Add(link2);
    }

    public Genome Crossover(Genome partner)
    {
        // determine relative fitness
        bool equalFitness = Mathf.Approximately(this.fitness, partner.fitness);
        Genome fitter = this.fitness > partner.fitness ? this : partner;
        Genome loser = this.fitness > partner.fitness ? partner : this;

        Genome child = new Genome(fitter.genomeID);

        // put loser genes in dict for quick matching
        Dictionary<int, ConnectionGene> loserGenes = new Dictionary<int, ConnectionGene>();
        foreach (var gene in loser.connections) loserGenes.Add(gene.innovationID, gene);

        // ingerit connections
        foreach (ConnectionGene fitterGene in fitter.connections)
        {
            if (loserGenes.ContainsKey(fitterGene.innovationID))
            {
                // matching: average weight
                ConnectionGene loserGene = loserGenes[fitterGene.innovationID];
                float averagedWeight = (fitterGene.weight + loserGene.weight) / 2f;
                
                bool isEnabled = true;
                if ((!fitterGene.enabled || !loserGene.enabled) && UnityEngine.Random.value < 0.75f)
                    isEnabled = false;

                child.connections.Add(new ConnectionGene(fitterGene.innovationID, fitterGene.inputNode, fitterGene.outputNode, averagedWeight, isEnabled));
            }
            else
            {
                // disjoint/excess: inherit from fitter
                child.connections.Add(new ConnectionGene(fitterGene.innovationID, fitterGene.inputNode, fitterGene.outputNode, fitterGene.weight, fitterGene.enabled));
            }
        }

        // if fitness is equal, also take disjoint genes from the loser
        if (equalFitness)
        {
            Dictionary<int, ConnectionGene> fitterGenes = new Dictionary<int, ConnectionGene>();
            foreach (var gene in fitter.connections) fitterGenes.Add(gene.innovationID, gene);

            foreach (ConnectionGene loserGene in loser.connections)
            {
                if (!fitterGenes.ContainsKey(loserGene.innovationID))
                {
                    child.connections.Add(new ConnectionGene(loserGene.innovationID, loserGene.inputNode, loserGene.outputNode, loserGene.weight, loserGene.enabled));
                }
            }
        }

        // node Inheritance
        HashSet<int> requiredNodes = new HashSet<int>();
        foreach (var conn in child.connections) { requiredNodes.Add(conn.inputNode); requiredNodes.Add(conn.outputNode); }

        // use combined list of nodes from both parents to make sure we find all ids
        List<NodeGene> allPossibleNodes = new List<NodeGene>(fitter.nodes);
        allPossibleNodes.AddRange(loser.nodes);

        foreach (var node in allPossibleNodes)
        {
            if (requiredNodes.Contains(node.innovationID))
            {
                // avoid duplicates
                if (child.nodes.Find(n => n.innovationID == node.innovationID) == null)
                    child.nodes.Add(new NodeGene(node.innovationID, node.nodeType, node.activation, node.bias));
            }
        }

        return child;
    }
    public float CompatibilityDistance(Genome otherGenome) {return 0f;}
    public void SortTopology() {}
}

[Serializable]
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

[Serializable]
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