using UnityEngine;
using System.Collections.Generic;

public class Innovation {
    public int innovationTracker;
    public Dictionary<int, List<object>> innovationRecords = new Dictionary<int, List<object>>();

    // code later
    public int GetInnovationNumber(NodeGene nodeIn, NodeGene nodeOut, string mutationType) 
    {
        return 0; // returns a id for the specific muatiaon
    }
    public void SetNewConnection(NodeGene inputNode, NodeGene outputNode) {}
    public void SetNewNode(ConnectionGene splittedConnection) {}
}

public class SaveLoadManager : MonoBehaviour {
    public string savePath;
    public string fileNamePrefix;

    // code later
    public bool SaveCreatureStructure(Structure structure, string name) {return true;}
    public Structure LoadCreatureStructure(string name) {return null;}
    public bool SaveSimulationState(NEAT neatInstance, string name) {return true;}
    public NEAT LoadSimulationState(NEAT neatInstance, string name) {return null;}
    public void AutoSave(NEAT neatInstance) {}
}

public class CreatureValidator : MonoBehaviour {
    // code later
    public bool ValidateStructure(Structure structure) {return true;}
    public bool CheckCollision(Structure structure) {return true;}
    public bool CheckTopology(Structure structure) {return true;}
}