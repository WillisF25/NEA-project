using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Structure
{
    public List<Joint> joints = new List<Joint>();
    public List<Link> links = new List<Link>();

    public void AddJoint(Vector3 pos)
    {
        joints.Add(new Joint(joints.Count, pos));
    }
    public void AddLink(int linkID, Joint jointA, Joint jointB, LinkType type) 
    {
        links.Add(new Link(linkID, jointA, jointB, type));
    }
}

[Serializable]
public class Joint
{
    public int id;
    [NonSerialized]
    public List<Link> links = new List<Link>();
    public Vector3 position;
    public float radius;
    public float mass;
    public GameObject jointObject;
    public Rigidbody2D rb;
    public Collider collider;

    public Joint(int jointID, Vector3 pos) 
    {
        this.id = jointID;
        this.position = pos;
        this.radius = 0.5f; // Default values as per design
        this.mass = 1.0f;
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
    public float minLength;
    public float maxLength;
    public float strength;
    public float damping;

    public Link(int linkID, Joint jointA, Joint jointB, LinkType type) {
        this.linkID = linkID;
        this.jointA = jointA;
        this.jointB = jointB;
        this.type = type;
        this.length = Vector3.Distance(jointA.position, jointB.position);
    }

    public float CurrentLength() 
    { 
        return Vector3.Distance(jointA.position, jointB.position); 
    }
}