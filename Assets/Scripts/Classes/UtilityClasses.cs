using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Innovation {
    // increments when a unique mutation occurs
    public int innovationTracker = 0;
    
    // key: InnovationID
    // value: [string mutationType, int nodeInID, int nodeOutID]
    // or for nodes: [string mutationType, int oldConnectionID]
    public Dictionary<int, List<object>> innovationRecords = new Dictionary<int, List<object>>();

    // checks if this mutation has happend before this gen
    // returns the existing id if found, or new id if not
    public int GetInnovationNumber(int nodeInID, int nodeOutID, string mutationType) 
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

        // increment global tracker for next unique muataion
        innovationTracker++;

        return newID;
    }

    // helper for splitting a connection to add a node
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

    public void SetNewConnection(NodeGene inputNode, NodeGene outputNode) {}
    public void SetNewNode(ConnectionGene splittedConnection) {}
}

public class SaveLoadManager {
    public string savePath;
    public string fileNamePrefix;

    // takes data and wirtes to disk
    public static bool SaveCreatureStructure(CreatureData data, string fileName)
    {
        try 
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, json);
            Debug.Log("Saved to: " + path);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save: " + e.Message);
            return false;
        }
    }
    // reads from disk and gives back data
    public static CreatureData LoadCreatureStructure(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        
        if (!File.Exists(path)) // presence check
        {
            Debug.LogWarning("File not found: " + path);
            return null;
        }

        // read and parse json
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<CreatureData>(json);
    }
    // removes the file
    public static void DeleteSaveFile(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted file: " + path);
        }
    }
    public bool SaveSimulationState(NEAT neatInstance, string name) {return true;}
    public NEAT LoadSimulationState(NEAT neatInstance, string name) {return null;}
    public void AutoSave(NEAT neatInstance) {}
}

public class CreatureValidator {
    // code later
    public bool ValidateStructure(Structure structure) {return true;}
    public bool CheckCollision(Structure structure) {return true;}
    public bool CheckTopology(Structure structure) {return true;}
}