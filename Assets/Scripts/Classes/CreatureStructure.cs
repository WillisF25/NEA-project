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
    public void AddLink(int linkID, Joint jointA, Joint jointB) 
    {
        links.Add(new Link(linkID, jointA, jointB));
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
public class Link
{
    public int linkID;
    public Joint jointA;
    public Joint jointB;
    public float length;
    public bool isMuscle;
    public float minLength;
    public float maxLength;
    public float strength;
    public float damping;

    public Link(int linkID, Joint jointA, Joint jointB) {
        this.linkID = linkID;
        this.jointA = jointA;
        this.jointB = jointB;
        this.length = Vector3.Distance(jointA.position, jointB.position);
    }

    public float CurrentLength() 
    { 
        return Vector3.Distance(jointA.position, jointB.position); 
    }
    public void ApplyMuscleForce()
    {
        // later code
    }
}