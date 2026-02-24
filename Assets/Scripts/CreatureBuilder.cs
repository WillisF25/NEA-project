using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CreatureBuilder : MonoBehaviour
{   
    public enum BuildMode { Spawning, Linking }
    public BuildMode currentMode = BuildMode.Spawning;
    public LinkType currentLinkType = LinkType.Bone;
    public GameObject jointPrefab;
    public GameObject linkPrefab;
    public Structure currentStructure = new Structure();

    public bool isSimulating = false;

    // map gameobj to its data
    private Dictionary<GameObject, Joint> jointMap = new Dictionary<GameObject, Joint>();

    // to remember the fist click for a link
    private GameObject selectedJointA = null;
    
    void Update()
    {   
        // check if mouse is currenlty over a ui button/panel
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
        {
            return; // dont do anything if clicking ui
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        // get mouse pos
        Vector2 mousePos = Mouse.current.position.ReadValue();
        // convert screen pixel pos to world unit pos
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));
        worldPos.z = 0f; // force z = 0 to keep in 2d
        Vector2 clickPoint = new Vector2(worldPos.x, worldPos.y);

        // check mouse and joint overlap
        Collider2D hit = Physics2D.OverlapPoint(clickPoint);

        if (currentMode == BuildMode.Spawning)
        {
            // in spawning mode, only spawn
            if (hit == null) SpawnJoint(clickPoint);
        }
        else if (currentMode == BuildMode.Linking)
        {
            // in linking mode, only look for existing joints to link
            if (hit != null && jointMap.ContainsKey(hit.gameObject))
            {
                SelectedJoint(hit.gameObject);
            }
        }
    }
    void SpawnJoint(Vector2 pos)
    {
        // determine id before creating data
        int newID = currentStructure.joints.Count;
        Joint data = new Joint(newID, pos);

        // create obj in game
        GameObject newJoint = Instantiate(jointPrefab, pos, Quaternion.identity);
        newJoint.name = $"Joint_{newID}";

        // link them in both direcitons
        data.jointObject = newJoint;
        jointMap.Add(newJoint, data);

        currentStructure.joints.Add(data); // add to current structure data
        
        Debug.Log($"Spawned Joint {newID} at {pos}");
    }

    void SelectedJoint(GameObject jointObj)
    {
        // case 1: deselecting
        if (selectedJointA == jointObj)
        {
            selectedJointA.GetComponent<SpriteRenderer>().color = Color.white;
            selectedJointA = null;
            return;
        }

        // case 2: selecting first
        if (selectedJointA == null)
        {
            selectedJointA = jointObj;
            selectedJointA.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        // case 3: have first joint and click a diff joint
        else
        {
            CreateLink(selectedJointA, jointObj);
        }
    }

    void CreateLink (GameObject a, GameObject b)
    {   
        if (a == b) return;

        Joint jointA = jointMap[a];
        Joint jointB = jointMap[b];
        
        // check if link exist already
        Link existingLink = currentStructure.links.Find(l => 
            (l.jointA == jointA && l.jointB == jointB) || (l.jointA == jointB && l.jointB == jointA));

        if (existingLink != null)
        {
            // if same type, ignore
            if (existingLink.type == currentLinkType)
            {
                Debug.Log("Link already exists. Ignoring.");
                // reset ui selection and stop
                a.GetComponent<SpriteRenderer>().color = Color.white;
                selectedJointA = null;
                return;
            }

            // if different type, override
            Debug.Log($"Replacing {existingLink.type} with {currentLinkType}");
            RemoveLink(a, b, existingLink);
            currentStructure.links.Remove(existingLink);
            // continue to rest of func to create new one
        }

        // create and record link data
        int newID = currentStructure.links.Count;
        Link newLinkData = new Link(newID, jointA, jointB, currentLinkType);
        currentStructure.links.Add(newLinkData);
        
        // setup visuals (handled by LinkFollower)
        GameObject linkVisual = Instantiate(linkPrefab);
        linkVisual.GetComponent<LinkFollower>().SetTargets(a.transform, b.transform);

        // setup physics
        LineRenderer lr = linkVisual.GetComponent<LineRenderer>();
        lr.startColor = currentLinkType == LinkType.Muscle ? Color.red : Color.white;
        lr.endColor = lr.startColor;

        // add the Muscle or Bone script
        if (currentLinkType == LinkType.Muscle)
        {
            lr.startColor = Color.red;
            lr.endColor = Color.red;

            // add the Muscle script
            Muscle m = a.AddComponent<Muscle>();
            m.rbA = a.GetComponent<Rigidbody2D>();
            m.rbB = b.GetComponent<Rigidbody2D>();
            
            // define expansion limits based on initial drawn length
            m.minLength = newLinkData.length * 0.5f;
            m.maxLength = newLinkData.length * 1.5f;
            m.springForce = 60f; // adjust based on mass
            m.damping = 5f;
        }
        else // bone
        {
            lr.startColor = Color.white;
            lr.endColor = Color.white;

            DistanceJoint2D physicalLink = a.AddComponent<DistanceJoint2D>();
            physicalLink.connectedBody = b.GetComponent<Rigidbody2D>();

            // hard set length
            physicalLink.autoConfigureDistance = false;
            physicalLink.distance = newLinkData.length;
            physicalLink.maxDistanceOnly = false;
            physicalLink.autoConfigureConnectedAnchor = false;
        }

        // reste ui selection
        a.GetComponent<SpriteRenderer>().color = Color.white;
        selectedJointA = null;
    }
    void RemoveLink(GameObject a, GameObject b, Link link)
    {
        // find and destroy the the visual gameobj
        LinkFollower[] allVisuals = FindObjectsByType<LinkFollower>(FindObjectsSortMode.None);
        foreach (var vis in allVisuals)
        {
            if ((vis.startObj == a.transform && vis.endObj == b.transform) ||
                (vis.startObj == b.transform && vis.endObj == a.transform))
            {
                Destroy(vis.gameObject);
                break;
            }
        }

        // 2. remoce the physics components from 'a'
        // if it was Bone
        DistanceJoint2D[] dJoints = a.GetComponents<DistanceJoint2D>();
        foreach (var dj in dJoints)
        {
            if (dj.connectedBody == b.GetComponent<Rigidbody2D>())
            {
                Destroy(dj);
            }
        }

        // if it was Muscle
        Muscle[] muscles = a.GetComponents<Muscle>();
        foreach (var m in muscles)
        {
            if (m.rbB == b.GetComponent<Rigidbody2D>())
            {
                Destroy(m);
            }
        }
    }

    public void ClearAll()
    {   
        foreach (var pair in jointMap) Destroy(pair.Key); // destroy all joint gameobjs

        // Links are destoryed automatically, but we do this to be safe
        GameObject[] links = GameObject.FindGameObjectsWithTag("Link");
        foreach (GameObject l in links) Destroy(l);

        // reset data
        currentStructure = new Structure(); // wipe strucutre list
        selectedJointA = null; // clear seleciton
        isSimulating = false; // reset mode
        jointMap.Clear(); // clear the joint map
    }

    public void SaveCreature()
    {   
        if (currentStructure.joints.Count <= 0)
        {
            Debug.LogWarning("Can't save empty structure.");
            return;
        }

        // create the container
        CreatureData data = new CreatureData();

        // map the gameobj to its id for links later
        foreach (Joint j in currentStructure.joints)
        {
            // use latest position in case it moved
            Vector3 pos = j.jointObject.transform.position;
            data.joints.Add(new JointData(j.id, pos.x, pos.y));
        }

        // find all links and save their connections
        foreach (Link l in currentStructure.links)
        {
            
                data.links.Add(new LinkData(
                    l.linkID,                     
                    l.jointA.id, 
                    l.jointB.id, 
                    l.type.ToString(), 
                    l.length
                ));
            
        }

        // send data to manager
        SaveLoadManager.SaveCreatureStructure(data, "creature.json");
    }

    public void LoadCreature()
    {
        // ask manager for the data
        CreatureData data = SaveLoadManager.LoadCreatureStructure("creature.json");        
        
        // presence check
        if (data == null) return;

        // clear current scene
        ClearAll();

        // rebuild joints , and map ids to gameobjs
        Dictionary<int, GameObject> idToOgj = new Dictionary<int, GameObject>();

        foreach (JointData jData in data.joints)
        {
            // same as spawn logic
            Vector2 pos = new Vector2(jData.x, jData.y);

            // spawn manually to set the wanted immediately
            GameObject newJoint = Instantiate(jointPrefab, pos, Quaternion.identity);
            
            Joint jointLogic = new Joint(jData.id, pos);
            jointLogic.jointObject = newJoint; // link the data to the obj
            
            // add to all the relevant data structures
            currentStructure.joints.Add(jointLogic);
            jointMap.Add(newJoint, jointLogic); 
            idToOgj.Add(jData.id, newJoint);
        }

        // rebuild Links
        foreach (LinkData lData in data.links)
        {   
            // find current type for link creation
            currentLinkType = (lData.type == "Bone") ? LinkType.Bone : LinkType.Muscle;
            // create the link using the mapped gameobjs
            CreateLink(idToOgj[lData.sourceJointID], idToOgj[lData.targetJointID]);
        }
    }
        
    

    public void DeleteSaveFile()
    {
        SaveLoadManager.DeleteSaveFile("creature.json");
    }

    public void StartSimulation()
    {
        // Muscle presence check
        bool hasMuscle = false;
        foreach (Link l in currentStructure.links)
        {
            if (l.type == LinkType.Muscle) { hasMuscle = true; break; }
        }

        if (!hasMuscle)
        {
            Debug.LogWarning("Creatures that have no muscle will be unable to move.");
            // ui action
            return;
        }

        bool validStructure = ValidateStructure(currentStructure, out List<Joint> isolatedJoints);
        if (!validStructure)
        {
            Debug.LogError("Invalid creature structure.");

            // highlight disconnected ones red
            foreach (Joint j in isolatedJoints)
            {   
                if (j.jointObject != null)
                    j.jointObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            return;
        }

        // save the current creature so next scene can find it
        SaveCreature();

        // load simulation scene
        SceneManager.LoadScene("Simulation");
    }

        public void ToggleSimulation()
    {
        isSimulating = !isSimulating; // toggle

        // finds all joints (with tag Joint)
        GameObject[] allJoints = GameObject.FindGameObjectsWithTag("Joint");

        foreach (GameObject joint in allJoints)
        {
            Rigidbody2D rb = joint.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (isSimulating)
                {
                    // enable moving physics on the joint
                    rb.bodyType = RigidbodyType2D.Dynamic;
                }
                else
                {
                    // turn off moving physcis
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero; // stop current movement
                    rb.angularVelocity = 0f; // stop spin
                }
            }
        }
    }

        public bool ValidateStructure (Structure structure, out List<Joint> isolatedJoints)
    {
        isolatedJoints = new List<Joint>();

        // if 0, failed presence check
        // if 1, means no muscle
        if (structure.joints.Count <= 1) return false;

        // find root joint (joint with lowest id)
        Joint rootJoint = structure.joints[0];

        // use bfs to find all reachable joints
        HashSet<Joint> visited = new HashSet<Joint>();
        Queue<Joint> searchQueue = new Queue<Joint>();

        searchQueue.Enqueue(rootJoint);
        visited.Add(rootJoint);

        while (searchQueue.Count > 0)
        {
            Joint current = searchQueue.Dequeue();

            // look through all links to find neighbours
            foreach (Link link in structure.links) 
            {
                if (link.jointA == current && !visited.Contains(link.jointB)) 
                {
                    visited.Add(link.jointB);
                    searchQueue.Enqueue(link.jointB);
                }
                else if (link.jointB == current && !visited.Contains(link.jointA)) 
                {
                    visited.Add(link.jointA);
                    searchQueue.Enqueue(link.jointA);
                }
            }
        }

        // add to isolatedJoints list if not in visited
        foreach (Joint j in structure.joints)
        {
            if (!visited.Contains(j)) isolatedJoints.Add(j);
        }
        
        return isolatedJoints.Count == 0;
    }
    
    // UI button connectors
    public void SetModeSpawning() => currentMode = BuildMode.Spawning;
    public void SetModeLinking() => currentMode = BuildMode.Linking;

    public void SetTypeBone() => currentLinkType = LinkType.Bone;
    public void SetTypeMuscle() => currentLinkType = LinkType.Muscle;
}