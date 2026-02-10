using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CreatureBuilder : MonoBehaviour
{   
    public enum BuildMode { Spawning, Linking }
    public BuildMode currentMode = BuildMode.Spawning;
    public LinkType currentLinkType = LinkType.Bone;
    public GameObject jointPrefab;
    public GameObject linkPrefab;
    public Structure currentStructure = new Structure();

    // To remember the fist click for a link
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

        Joint dataA = a.GetComponent<JointIdentity>().jointData;
        Joint dataB = b.GetComponent<JointIdentity>().jointData;

        // create link data
        int newID = currentStructure.links.Count;
        Link newLinkData = new Link(newID, dataA, dataB, currentLinkType);
        currentStructure.links.Add(newLinkData);
        
        // physcis setup
        DistanceJoint2D physicalLink = a.AddComponent<DistanceJoint2D>();
        physicalLink.connectedBody = b.GetComponent<Rigidbody2D>();
        physicalLink.distance = newLinkData.length;
        
        // set the distance to length calced in Link
        physicalLink.distance = newLinkData.length;

        // visulas and components
        if (linkPrefab != null)
        {
            GameObject linkVisual = Instantiate(linkPrefab);
            linkVisual.GetComponent<LinkFollower>().SetTargets(a.transform, b.transform);
            LineRenderer lr = linkVisual.GetComponent<LineRenderer>();

            if (currentLinkType == LinkType.Muscle)
            {
                // set muscle visual
                lr.startColor = Color.red;
                lr.endColor = Color.red;

                // add muscle script
                Muscle m = a.AddComponent<Muscle>();
                m.joint = physicalLink;
                m.minLength = physicalLink.distance * 0.7f; // default to 70% of original
                m.maxLength = physicalLink.distance * 1.3f; // default to 130% of original
                m.strength = 5.0f;
                m.damping = 0.5f;
            }
            else
            {
                // Set Bone Visuals
                lr.startColor = Color.white;
                lr.endColor = Color.white;
            }
        }

        a.GetComponent<SpriteRenderer>().color = Color.white;
        selectedJointA = null;
    }

    // UI button connectors
    public void SetModeSpawning() => currentMode = BuildMode.Spawning;
    public void SetModeLinking() => currentMode = BuildMode.Linking;

    public void SetTypeBone() => currentLinkType = LinkType.Bone;
    public void SetTypeMuscle() => currentLinkType = LinkType.Muscle;
}