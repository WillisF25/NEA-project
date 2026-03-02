using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Root container for a creature's blueprint, used for JSON saving and loading.
/// </summary>
[Serializable]
public class Structure
{   
    /// <summary>
    /// List of all joints in the creature.
    /// </summary>
    public List<Joint> joints = new List<Joint>();
    /// <summary>
    /// List of all physical connections (Bones/Muscles).
    /// </summary>
    public List<Link> links = new List<Link>();
}

/// <summary>
/// Blueprint data for a single physical joint.
/// </summary>
[Serializable]
public class Joint
{
    public int id;
    /// <summary>
    /// The local position in the builder. NonSerialized as we save
    /// individual coordinates for clearner JSON formatting.
    /// </summary>
    [NonSerialized]
    public Vector3 position;
    public float radius;
    public float mass;
    public GameObject jointObject;

    /// <summary>
    /// Constructor to initialise a new joint with default physics values.
    /// </summary>
    /// <param name="jointID">Unique identifier for this joint.</param>
    /// <param name="pos">The 3D coordinate where this joint was placed.</param>
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

/// <summary>
/// Defines the relationship and properties of a connection between two joints.
/// </summary>
[Serializable]
public class Link
{
    public int linkID;
    public Joint jointA;
    public Joint jointB;
    /// <summary>
    /// The resting distance calculated at the time of creation.
    /// </summary>
    public float length;
    /// <summary>
    /// Determines if this link is a rigid Bone or a contollable Muscle.
    /// </summary>
    public LinkType type;

    /// <summary>
    /// Constuctor that links tow joints and calculates the distance between them.
    /// </summary>
    /// <param name="linkID">Unique identifier for this link.</param>
    public Link(int linkID, Joint jointA, Joint jointB, LinkType type) {
        this.linkID = linkID;
        this.jointA = jointA;
        this.jointB = jointB;
        this.type = type;
        length = Vector3.Distance(jointA.position, jointB.position);
    }
}