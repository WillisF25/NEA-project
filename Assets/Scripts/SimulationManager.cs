using UnityEngine;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour
{
    public GameObject jointPrefab;
    public GameObject linkPrefab;

    // NEAT variables
    public NEAT neatSystem;
    public int populationSize = 50; // default for now
    public float generationTimeLimit = 15f;
    private float globalTimer;

    private CreatureData data;
    
    // use this to tell the camera which joint to follow
    public static Transform focusTarget; 

    void Start()
    {   
        // load the structure blueprint
        data = SaveLoadManager.LoadCreatureStructure("creature.json");
        if (data == null)
        {
            Debug.LogError("No creature data found.");
            return;
        }

        // count inputs (joints) and outputs (muscles) from bluprint
        int jointCount = data.joints.Count;
        int muscleCount = 0;
        foreach (var lData in data.links)
        {
            if (lData.type == "Muscle") muscleCount++;
        }

        // initialise NEAT
        neatSystem = new NEAT();
        neatSystem.populationLimit = populationSize;
        neatSystem.trainingGoal = "Walk Right";
        neatSystem.generationNumber = 0;

        // initialise gen 0
        neatSystem.InitialisePopulation(jointCount, muscleCount);

        // start gen 0
        SpawnPopulation();
        globalTimer = generationTimeLimit;
        Debug.Log($"Started Generation {neatSystem.generationNumber}");
    }

    void Update()
    {
        // timer
        globalTimer -= Time.deltaTime;

        if (globalTimer <= 0)
        {
            AdvanceGeneration();
        }
    }

    void SpawnPopulation()
    {
        foreach (Creature c in neatSystem.population)
        {
            AssembleCreature(c.genome);
        }
    }

    void AdvanceGeneration()
    {
        // evaluate
        CreatureFollower[] followers = Object.FindObjectsByType<CreatureFollower>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        neatSystem.EvaluateFitness(followers);

        // evolve
        neatSystem.Speciate();
        neatSystem.AdjustFitness();
        neatSystem.Reproduce();

        // clean up old gen's bodies
        GameObject[] joints = GameObject.FindGameObjectsWithTag("Joint");
        GameObject[] links = GameObject.FindGameObjectsWithTag("Link");
        foreach (GameObject j in joints) Destroy(j);
        foreach (GameObject l in links) Destroy(l);

        // spawn in new gen
        SpawnPopulation();

        // reset timer
        globalTimer = generationTimeLimit;
        
        // increment generation number
        neatSystem.generationNumber++;
        Debug.Log($"Started Generation {neatSystem.generationNumber}");
    }

    void AssembleCreature(Genome genome)
    {
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
            
            // set first joint as camera target, right now it only follows the very first creature spawned
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

        // attach the follower script (containing the brain) to the first joint
        GameObject firstJoint = loadedJoints[data.joints[0].id];
        CreatureFollower follower = firstJoint.AddComponent<CreatureFollower>();

        // start the brain
        follower.Init(genome, spawnedMuscles, spawnedJoints);
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