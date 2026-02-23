using UnityEngine;
using System.IO;

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
