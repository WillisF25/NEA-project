using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Structure
{
    public List<Joint> joints = new List<Joint>();
    public List<Link> links = new List<Link>();
}

[Serializable]
public class Joint
{
    public int id;
    [NonSerialized]
    public Vector3 position;
    public float radius;
    public float mass;
    public GameObject jointObject;

    public Joint(int jointID, Vector3 pos) 
    {
        id = jointID;
        position = pos;
        radius = 0.5f; // Default values as per design
        mass = 1.0f;
    }
}

[Serializable]
public enum LinkType { Bone, Muscle }

[Serializable]
public class Link
{
    public int linkID;
    public Joint jointA;
    public Joint jointB;
    public float length;
    public LinkType type;

    public Link(int linkID, Joint jointA, Joint jointB, LinkType type) {
        this.linkID = linkID;
        this.jointA = jointA;
        this.jointB = jointB;
        this.type = type;
        length = Vector3.Distance(jointA.position, jointB.position);
    }
}