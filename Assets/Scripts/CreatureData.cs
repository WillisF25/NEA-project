using System.Collections.Generic;

/// <summary>
/// The root data contianer for a creature's physcial blueprint.
/// Designed for JSON serialisation to allow saving and loading of designs.
/// </summary>
[System.Serializable]
public class CreatureData
{   
    /// <summary> A list of all vertex points (joints) that make up the creature's structure. </summary>
    public List<JointData> joints = new List<JointData>();
    /// <summary> A list of all connections (bones/muscles) that link the joints together. </summary>
    public List<LinkData> links = new List<LinkData>();
}

/// <summary>
/// Simplified representation of a Joint for storage.
/// Uses raw floats instead of Vector2 to ensure consistent JSON formatting.
/// </summary>
[System.Serializable]
public class JointData
{   
    /// <summary> Unique identifier used to link this joint to connections. </summary>
    public int id;
    /// <summary> Horizontal position in the Builder canvas. </summary>
    public float x;
    /// <summary> Vertical position in the Builder canvas. </summary>
    public float y;
    
    /// <summary>
    /// Initializes a new data snapshot of a joint.
    /// </summary>
    /// <param name="id">Unique ID assigned by the BuilderManager.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public JointData(int id, float x, float y)
    {
        this.id = id;
        this.x = x;
        this.y = y;
    }
}

/// <summary>
/// Simplified representation of a Link for storage.
/// Uses ID references rather than direct object references to avoid circular JSON dependencies.
/// </summary>
[System.Serializable]
public class LinkData
{   
    /// <summary> Unique identifier for this specific connection. </summary>
    public int id;
    /// <summary> The ID of the joint where the link starts. </summary>
    public int sourceJointID;
    /// <summary> The ID of the joint where the link ends. </summary>
    public int targetJointID;
    /// <summary> Defines the physical behavior.</summary>
    /// <remarks> Expected values: "Bone" or "Muscle". </remarks>
    public string type;
    /// <summary> The resting distance between the two joints. </summary>
    public float length;

    /// <summary>
    /// Initializes a new data snapshot of a link.
    /// </summary>
    /// <param name="id">Unique ID for this link.</param>
    /// <param name="sourceID">ID of the starting joint.</param>
    /// <param name="targetID">ID of the target joint.</param>
    /// <param name="type">Behavior type ("Bone" / "Muscle").</param>
    /// <param name="length">Initial resting distance.</param>
    public LinkData(int id, int sourceID, int targetID, string type, float length)
    {
        this.id = id;
        sourceJointID = sourceID;
        targetJointID = targetID;
        this.type = type;
        this.length = length;
    }
}