using UnityEngine;
using System.IO;

/// <summary>
/// A static utility class that handles reading and writing creature data to the local drive.
/// Uses Unity's JsonUtility for high performance serialisation.
/// </summary>
public class SaveLoadManager {
    public string savePath;
    public string fileNamePrefix;

    /// <summary>
    /// Converts a CreatureData object into a JSON string and writes it to the persistent data path.
    /// </summary>
    /// <param name="data">The physical structure of the creature to save.</param>
    /// <param name="fileName">The name of the file (e.g., "creature.json").</param>
    /// <returns>True if save was successful, false if an error occurred.</returns>
    /// <remarks>
    /// We use a try catch block here because disk I/O can fail for many reasons outside 
    /// our control (e.g., drive is full, permissions are denied).
    /// </remarks>
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
    
    /// <summary>
    /// Locates a file on the drive, reads its contents, and reconstructs a CreatureData object.
    /// </summary>
    /// <param name="fileName">The name of the file to look for.</param>
    /// <returns>A populated CreatureData object if found, else return null.</returns>
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
    
    /// <summary>
    /// Permanently removes a save file from the user's drive.
    /// </summary>
    /// <param name="fileName">The specific file to delete.</param>
    public static void DeleteSaveFile(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted file: " + path);
        }
    }
    //placemoder methods for furture neat
    public bool SaveSimulationState(NEAT neatInstance, string name) {return true;}
    public NEAT LoadSimulationState(NEAT neatInstance, string name) {return null;}
    public void AutoSave(NEAT neatInstance) {}
}
