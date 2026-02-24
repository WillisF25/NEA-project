using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SimulationManager : MonoBehaviour
{   
    [Header("Prefabs")]
    public GameObject jointPrefab;
    public GameObject linkPrefab;
    public Transform recordFlag; 

    // where the json loads to and holds the data for the structure
    private CreatureData data;

    // NEAT variables
    public NEAT neatSystem;
    public float generationTimeLimit;
    private float globalTimer;
    public TextMeshProUGUI timerDisplay;
    
    // camrea variables
    public enum CameraMode { ShowAll, ShowBestOnly, HighlightBest }
    public CameraMode currentCameraMode = CameraMode.HighlightBest; // default to highlight best
    public static Transform focusTarget; 
    private CreatureFollower[] activeCreatures;

    // track how far creatures have ever gotten
    private float allTimeHigh = 0f;


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
        neatSystem.generationNumber = 0;

        // initialise gen 0
        neatSystem.InitialisePopulation(jointCount, muscleCount);

        // 1. SPAWN THE FLAG AT THE START
        if (recordFlag != null)
        {
            Transform flagObj = Instantiate(recordFlag, new Vector3(0, 0, 0), Quaternion.identity);
            recordFlag = flagObj.transform;
        }

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

        UpdateCamreaTarget();
        UpdateRecordVisuals();
        UpdateTimerUI();
    }

    void SpawnPopulation()
    {
        foreach (Genome g in neatSystem.population)
        {
            AssembleCreature(g);
        }

        activeCreatures = FindObjectsByType<CreatureFollower>(FindObjectsSortMode.None);
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

        // clear camrea target array
        activeCreatures = null;
        focusTarget = null;

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
        List<LineRenderer> spawnedLines = new List<LineRenderer>();
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
            rb.freezeRotation = true; // prevent joints acting as wheels

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
                Muscle m = CreatePhysicalLink(loadedJoints[lData.sourceJointID], loadedJoints[lData.targetJointID], lData, spawnedLines);
                if (m != null) spawnedMuscles.Add(m);
            }
        }

        // attach the follower script (containing the brain) to the first joint
        GameObject firstJoint = loadedJoints[data.joints[0].id];
        CreatureFollower follower = firstJoint.AddComponent<CreatureFollower>();

        // start the brain
        follower.Init(genome, spawnedMuscles, spawnedJoints, spawnedLines);
    }

    Muscle CreatePhysicalLink(GameObject a, GameObject b, LinkData lData, List<LineRenderer> spawnedLines)
    {
        // visuals 
        GameObject linkVisual = Instantiate(linkPrefab);
        linkVisual.GetComponent<LinkFollower>().SetTargets(a.transform, b.transform);
        LineRenderer lr = linkVisual.GetComponent<LineRenderer>();

        // add lr to list for camrea logic use
        spawnedLines.Add(lr);

        if (lData.type == "Muscle")
        {   
            // muscle visuals
            lr.startColor = Color.red;
            lr.endColor = Color.red;

            // add muscle script
            Muscle m = a.AddComponent<Muscle>();
            m.rbA = a.GetComponent<Rigidbody2D>();
            m.rbB = b.GetComponent<Rigidbody2D>();

            // use saved length form json
            m.minLength = lData.length * 0.5f;
            m.maxLength = lData.length * 1.5f;
            m.springForce = 100f; // muscle strength
            m.damping = 10f; // stop jitter

            return m; // return muscle so it can be added to nn controller list
        }
        else // bone
        {
            // bone visuals
            lr.startColor = Color.white;
            lr.endColor = Color.white;

            // add DistanceJoint2D
            DistanceJoint2D physicalLink = a.AddComponent<DistanceJoint2D>();
            physicalLink.connectedBody = b.GetComponent<Rigidbody2D>();

            // ensure the bone is rigid and matches the saved blueprint
            physicalLink.autoConfigureDistance = false;
            physicalLink.distance = lData.length;
            physicalLink.maxDistanceOnly = false;
            physicalLink.autoConfigureConnectedAnchor = false;

            // set anchors to center to prevent weird offsets
            physicalLink.anchor = Vector2.zero;
            physicalLink.connectedAnchor = Vector2.zero;

            return null; // bones are not controlled by nn
        }
    }

        public void BackToBuilder()
        {
            // load builder scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("CreatureBuilder");
        }

    // timer ui
    void UpdateTimerUI()
    {
        // formatting: "00:00"
        float minutes = Mathf.FloorToInt(globalTimer / 60);
        float seconds = Mathf.FloorToInt(globalTimer % 60);
        
        // ensure the timer doesn't display negative numbers visually
        float displayTime = Mathf.Max(0, globalTimer);
        
        timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // camrea logic
    void UpdateCamreaTarget()
    {
        if (activeCreatures == null || activeCreatures.Length == 0) return;

        CreatureFollower bestCreature = null;
        float maxFitness = -float.MaxValue;

        // find best creature
        for (int i = 0; i < activeCreatures.Length; i++)
        {
            // check if the creature still exists in the scene
            if (activeCreatures[i] == null) continue;

            // compare fitness
            if (activeCreatures[i].currentFitness > maxFitness)
            {
                maxFitness = activeCreatures[i].currentFitness;
                bestCreature = activeCreatures[i];
            }
        }

        // only update the target if found a living creature
        if (bestCreature != null)
        {
            focusTarget = bestCreature.transform;
        }

        // apply visibilty modes
        foreach (CreatureFollower creature in activeCreatures)
        {
            if (creature == null) continue;

            bool isBest = creature == bestCreature;

            switch (currentCameraMode)
            {
                case CameraMode.ShowAll:
                    creature.SetVisibility(1.0f); // fully visible
                    break;

                case CameraMode.ShowBestOnly:
                    creature.SetVisibility(isBest ? 1.0f : 0.0f); // hide others
                    break;

                case CameraMode.HighlightBest:
                    creature.SetVisibility(isBest ? 1.0f : 0.2f); // fade others
                    break;
            }
        }
    }

    // update the marker showing the all time high
    void UpdateRecordVisuals()
    {
        float currentMaxFitness = -float.MaxValue;

        // find the current best distance in this gen
        foreach (CreatureFollower creature in activeCreatures)
        {
            if (creature != null && creature.currentFitness > currentMaxFitness)
            {
                currentMaxFitness = creature.currentFitness;
            }
        }

        // if we just broke the all time record, move the flag
        if (currentMaxFitness > allTimeHigh)
        {
            allTimeHigh = currentMaxFitness;
            
            if (recordFlag != null)
            {
                // move the flag to the new x pos
                recordFlag.position = new Vector3(allTimeHigh, recordFlag.position.y, 0);
            }
        }
    }
}