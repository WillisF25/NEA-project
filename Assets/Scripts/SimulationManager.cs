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
        //AssembleCreature();
    }

    void AssembleCreature(Genome brainGenome)
    {
        // get data
        CreatureData data = SaveLoadManager.LoadCreatureStructure("creature.json");
        if (data == null) return;

        // list to hold brain refs
        List<Transform> spawnedJoints = new List<Transform>();
        List<Muscle> spawnedMuscles = new List<Muscle>();
        Dictionary<int, GameObject> loadedJoints = new Dictionary<int, GameObject>();

        // spawn joint
        foreach (var jData in data.joints)
        {
            Vector2 pos = new Vector2(jData.x, jData.y);
            GameObject jObj = Instantiate(jointPrefab, pos, Quaternion.identity);
            
            // adding the rb in scene
            Rigidbody2D rb = jObj.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic; 
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // keep creature simulated

            loadedJoints.Add(jData.id, jObj);
            spawnedJoints.Add(jObj.transform); // add to list for brain
            
            // set first joint as camera target
            if (focusTarget == null) focusTarget = jObj.transform;
        }

        // spawn Links
        foreach (var lData in data.links)
        {
            if (loadedJoints.ContainsKey(lData.sourceJointID) && loadedJoints.ContainsKey(lData.targetJointID))
            {
                Muscle m = CreatePhysicalLink(loadedJoints[lData.sourceJointID], loadedJoints[lData.targetJointID], lData);
                if (m != null) spawnedMuscles.Add(m);
            }
        }

        // attach the brain to the first joint
        GameObject firstJoint = loadedJoints[data.joints[0].id];
        CreatureBrain brain = firstJoint.AddComponent<CreatureBrain>();

        // start the brain
        brain.Init(brainGenome, spawnedMuscles, spawnedJoints);
    }

    Muscle CreatePhysicalLink(GameObject a, GameObject b, LinkData lData)
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

            return m; // return muscle component
        }
        else
        {
            lr.startColor = Color.white;
            lr.endColor = Color.white;
        }

        return null; // return null if its bone
    }

        public void BackToBuilder()
        {
            // load builder scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("CreatureBuilder");
        }
}