using UnityEngine;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour
{
    public GameObject jointPrefab;
    public GameObject linkPrefab;
    
    // use this to tell the camera which joint to follow
    public static Transform focusTarget; 

    void Start()
    {
        AssembleCreature();
    }

    void AssembleCreature()
    {
        // get data
        CreatureData data = SaveLoadManager.LoadCreatureStructure("creature.json");
        if (data == null) return;

        Dictionary<int, GameObject> loadedJoints = new Dictionary<int, GameObject>();

        // spawn joint
        foreach (var jData in data.joints)
        {
            Vector2 pos = new Vector2(jData.x, jData.y);
            GameObject jObj = Instantiate(jointPrefab, pos, Quaternion.identity);
            
            // adding the rb in scene
            Rigidbody2D rb = jObj.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic; 

            loadedJoints.Add(jData.id, jObj);
            
            // set first joint as camera target
            if (focusTarget == null) focusTarget = jObj.transform;
        }

        // spawn Links
        foreach (var lData in data.links)
        {
            if (loadedJoints.ContainsKey(lData.sourceJointID) && loadedJoints.ContainsKey(lData.targetJointID))
            {
                CreatePhysicalLink(loadedJoints[lData.sourceJointID], loadedJoints[lData.targetJointID], lData);
            }
        }
    }

    void CreatePhysicalLink(GameObject a, GameObject b, LinkData lData)
    {
        // physics setup
        DistanceJoint2D physicalLink = a.AddComponent<DistanceJoint2D>();
        physicalLink.connectedBody = b.GetComponent<Rigidbody2D>();
        physicalLink.distance = lData.length;

        // visuals 
        GameObject linkVisual = Instantiate(linkPrefab);
        linkVisual.GetComponent<LinkFollower>().SetTargets(a.transform, b.transform);
        LineRenderer lr = linkVisual.GetComponent<LineRenderer>();

        if (lData.type == "Muscle")
        {
            lr.startColor = Color.red;
            lr.endColor = Color.red;

            // add muscle script
            Muscle m = a.AddComponent<Muscle>();
            m.joint = physicalLink;
            m.minLength = lData.length * 0.7f;
            m.maxLength = lData.length * 1.3f;
        }
        else
        {
            lr.startColor = Color.white;
            lr.endColor = Color.white;
        }
    }
}