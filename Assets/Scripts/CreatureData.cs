using System.Collections.Generic;

[System.Serializable]
public class CreatureData
{
    public List<JointData> joints = new List<JointData>();
    public List<LinkData> links = new List<LinkData>();
}

[System.Serializable]
public class JointData
{
    public int id;
    public float x;
    public float y;

    public JointData(int id, float x, float y)
    {
        this.id = id;
        this.x = x;
        this.y = y;
    }
}

[System.Serializable]
public class LinkData
{
    public int id;
    public int sourceJointID; // joint a
    public int targetJointID; // joint b
    public string type;       // "Bone" or "Muscle"
    public float length;      // initial length

    public LinkData(int id, int sourceID, int targetID, string type, float length)
    {
        this.id = id;
        sourceJointID = sourceID;
        targetJointID = targetID;
        this.type = type;
        this.length = length;
    }
}