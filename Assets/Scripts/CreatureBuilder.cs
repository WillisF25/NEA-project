using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;

public class CreatureBuilder : MonoBehaviour
{   
    public enum BuildMode { Spawning, Linking }
    public BuildMode currentMode = BuildMode.Spawning;
    public GameObject jointPrefab;
    public GameObject linkPrefab;
    public Structure currentStructure = new Structure();

    // To remember the fist click for a link
    private GameObject selectedJointA = null;
    
    void Update()
    {
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
            if (hit != null && hit.CompareTag("Joint"))
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
    currentStructure.joints.Add(data);

    // create obj in game
    GameObject newJoint = Instantiate(jointPrefab, pos, Quaternion.identity);
    newJoint.name = $"Joint_{newID}";

    JointIdentity identity = newJoint.GetComponent<JointIdentity>();

    // add new if prefab didnt have one already
    if (identity == null)
        {
            identity = newJoint.AddComponent<JointIdentity>();
        }
    
    // assign data to script
    identity.jointData = data;
    
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
        if (linkPrefab == null) 
        {
            Debug.LogError("Link Prefab is missing! Assign it in the Inspector.");
            return;
        }

        Joint dataA = a.GetComponent<JointIdentity>().jointData;
        Joint dataB = b.GetComponent<JointIdentity>().jointData;

        // data struct logic
        int newID = currentStructure.links.Count;
        Link newLinkData = new Link(newID, dataA, dataB);
        currentStructure.links.Add(newLinkData);
        
        // physcis logic
        DistanceJoint2D physicalLink = a.AddComponent<DistanceJoint2D>();
        physicalLink.connectedBody = b.GetComponent<Rigidbody2D>();
        physicalLink.distance = newLinkData.length;
        
        // set the distance to length calced in Link
        physicalLink.distance = newLinkData.length;

        // visuals
        GameObject linkVisual = Instantiate(linkPrefab);
        linkVisual.name = $"Link_{dataA.id}_{dataB.id}";

        // connect visuals
        LinkFollower follower = linkVisual.GetComponent<LinkFollower>();
        if (follower != null)
        {
            follower.SetTargets(a.transform, b.transform);
        }

        Debug.Log($"Created Link {newID} between Joint {dataA.id} and {dataB.id}");

        // reset select
        a.GetComponent<SpriteRenderer>().color = Color.white;
        selectedJointA = null;
    }
}
