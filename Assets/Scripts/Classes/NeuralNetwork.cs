using System.Collections.Generic;

public class NeuralNetwork {
    private List<Node> nodes = new List<Node>();
    private List<Connection> connections = new List<Connection>();

    public NeuralNetwork(Genome genome) {
        // logic to convert gene to functional nn stuff
    }

    public List<float> ForwardPass(List<float> inputs) 
    { 
        return new List<float>(); // code later
    }
}

public class Node {
    private int nodeID;
    private string nodeType;
    private float bias;
    
    public delegate float ActivationFunction(float x);
    private ActivationFunction activationFunctionPtr;
    
    private float inputSum;
    private float output;

    public int NodeID => nodeID;
    public string NodeType => nodeType;
    public float Output => output;

    public Node(NodeGene gene) 
    {
        this.nodeID = gene.innovationID;
        this.nodeType = gene.nodeType;
        this.bias = gene.bias;
        this.inputSum = 0f;
        this.output = 0f;
    }
}

public class Connection {
    private int inputNode;
    private int outputNode;
    private float weight;
    private bool enabled;

    public float Weight => weight;
    public bool Enabled => enabled;

    public Connection(Node nodeA, Node nodeB, float w) 
    {
        this.inputNode = nodeA.NodeID;
        this.outputNode = nodeB.NodeID;
        this.weight = w;
        this.enabled = true;
    }
}