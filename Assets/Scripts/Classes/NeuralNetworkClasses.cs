using System;
using System.Collections.Generic;

public class NeuralNetwork {
    private List<Node> nodes = new List<Node>();
    private List<Connection> connections = new List<Connection>();

    public NeuralNetwork(Genome genome) {
        // create Nodes and Connections from genes
        // link them
    }

    public List<float> ForwardPass(List<float> inputs) 
    { 
        // set innodes outputs
        // calc hidden/ output nodes
        return new List<float>(); 
    }
}

// Phenotype components
public class Node {
    private int nodeID;
    private string nodeType;
    private float bias;
    
    public delegate float ActivationFunction(float x);
    private ActivationFunction activationFunctionPtr;
    
    private float inputSum;
    private float output;

    // getters
    public int NodeID => nodeID;
    public string NodeType => nodeType;
    public float Output { get => output; set => output = value; }
    public float InputSum { get => inputSum; set => inputSum = value; }

    // store refs to conns feeding into this node
    public List<Connection> IncomingConnections = new List<Connection>();

    public Node(NodeGene gene) 
    {
        this.nodeID = gene.innovationID;
        this.nodeType = gene.nodeType;
        this.bias = gene.bias;
        this.inputSum = 0f;
        this.output = 0f;

        // map sting name from gene to actual math function
        this.activationFunctionPtr = GetActivation(gene.activation);
    }

    public void Calculate()
    {   // start with the defined bias
        float sum = bias;

        // sum up signals from enabled connections
        foreach (var conn in IncomingConnections)
        {
            if (conn.Enabled)
            {
                sum += conn.InputNode.Output * conn.Weight;
            }
        }

        inputSum = sum;
        output = activationFunctionPtr(inputSum);
    }

    // helper to resolve delegates
    private ActivationFunction GetActivation(string name) 
    {
        switch (name.ToLower()) {
            case "sigmoid": return (x) => 1f / (1f + (float)Math.Exp(-x));
            case "relu": return (x) => Math.Max(0, x);
            case "linear": return (x) => x;
            default: return (x) => (float)Math.Tanh(x);
        }
    }
}

public class Connection {
    // refs to node objs
    private Node inputNode;
    private Node outputNode;

    private float weight;
    private bool enabled;

    public float Weight => weight;
    public bool Enabled => enabled;
    public Node InputNode => inputNode;
    public Node OutputNode => outputNode;

    public Connection(Node from, Node to, ConnectionGene gene) 
    {
        this.inputNode = from;
        this.outputNode = to;
        this.weight = gene.weight;
        this.enabled = gene.enabled;
    }
}