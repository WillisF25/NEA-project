using System;
using System.Collections.Generic;

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

    // later code
    public void Mutate() { }
    public void AddConnection() { }
    public void AddNode() { }
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