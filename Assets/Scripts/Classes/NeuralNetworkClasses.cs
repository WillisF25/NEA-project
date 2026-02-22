using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork {
    private List<Node> nodes = new List<Node>();
    private List<Node> inputNodes = new List<Node>();
    private List<Node> outputNodes = new List<Node>();
    //private List<Connection> connections = new List<Connection>();

    // reusable output buffer
    private float[] outputBuffer;

    public NeuralNetwork(Genome genome) {
        // CREATE NODES
        // a temp dict to find Node obj using its id
        Dictionary<int, Node> nodeMap = new Dictionary<int, Node>();

        // instantiate the neurons
        foreach (NodeGene gene in genome.nodes)
        {
            // Check if the node is already in the map to prevent crashes
            if (nodeMap.ContainsKey(gene.innovationID)) 
            {
                Debug.LogWarning($"Duplicate Node ID {gene.innovationID} found in Genome {genome.genomeID}. Skipping.");
                continue; 
            }

            // create the Node form blueprint
            Node newNode = new Node(gene);
            nodes.Add(newNode);

            // add to registry
            nodeMap.Add(gene.innovationID, newNode);

            // categorise
            if (gene.nodeType == "INPUT") inputNodes.Add(newNode);
            else if (gene.nodeType == "OUTPUT") outputNodes.Add(newNode);
        }

        // LINK CONNECTIONS
        foreach (ConnectionGene gene in genome.connections)
        {
            if (gene.enabled)
            {
                // presence check for both nodes
                if (nodeMap.ContainsKey(gene.inputNode) && nodeMap.ContainsKey(gene.outputNode))
                {
                    Node input = nodeMap[gene.inputNode];
                    Node output = nodeMap[gene.outputNode];

                    // create the conn obj
                    Connection newConn = new Connection(input, output, gene);

                    // tell destination node it has a new incoming conn
                    output.IncomingConnections.Add(newConn);
                }

            }
        }

        SortTopology();

        outputBuffer = new float[outputNodes.Count];
    }

    public float[] ForwardPass(float[] inputs) 
    { 
        // ensure num of sensors on the creature matches with num of input neurons
        if (inputs.Length != inputNodes.Count)
        {   
            // if mismatch
            return null;
        }

        // inject the inputs
        for (int i=0; i < inputNodes.Count; i++)
        {
            inputNodes[i].Output = inputs[i];
            inputNodes[i].InputSum = inputs[i];
        }

        // calculation loop
        foreach (Node node in nodes)
        {
            if (node.NodeType != "INPUT")
            {
                node.Calculate();
            }
        }

        // get outputs
        for (int i = 0; i < outputNodes.Count; i++)
        {
            outputBuffer[i] = outputNodes[i].Output;
        }

        return outputBuffer;
    }

    public void SortTopology()
    {
        // reset depths
        foreach (var node in nodes)
        {
            if (node.NodeType == "INPUT") node.Depth = 0;
            else node.Depth = -1; // uncalculated depth
        }

        // assign depths to Hidden and Output nodes
        bool changed = true;
        int safetyIterator = 0;

        // loop unitl no more depths change or hit a safety limit (circular refs)
        while (changed && safetyIterator < nodes.Count)
        {
            changed = false;
            foreach(var node in nodes)
            {
                if(node.NodeType == "INPUT") continue;

                float maxParentDepth = -1;
                foreach (var conn in node.IncomingConnections)
                {
                    if (conn.Enabled && conn.InputNode.Depth != -1)
                    {
                        maxParentDepth = MathF.Max(maxParentDepth, conn.InputNode.Depth);
                    }
                }

                // if found vaild parent, this node's depth is parent+1
                if (maxParentDepth != -1)
                {
                    float newDepth = maxParentDepth + 1;
                    if (node.Depth != newDepth)
                    {
                        node.Depth = newDepth;
                        changed = true;
                    }
                }
            }
            safetyIterator++;
        }

        // force Output nodes to be last thing to be calculated
        foreach (var node in outputNodes)
        {
            node.Depth = 1000; // arbitrary high number
        }

        // sort the list based on the depths
        nodes.Sort((a, b) => a.Depth.CompareTo(b.Depth));
    }
}

// Phenotype components
public class Node {
    private int nodeID;
    private string nodeType;
    private float bias;
    
    // 0 for input, 1 or ouput, something in between for hidden
    public float Depth;
    
    private float inputSum;
    private float output;

    public delegate float ActivationFunction(float x);
    private ActivationFunction activationFunctionPtr;

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