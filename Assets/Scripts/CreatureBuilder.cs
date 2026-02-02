using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CreatureBuilder : MonoBehaviour
{   
    [Header("Settings")]
    public Camera mainCamera;
    public GameObject jointPrefab;
    [Header("Data")]
    public Structure currentStructure = new Structure();
    // To remember the fist click for a link
    private GameObject selectedJointA = null;

    public void Onclick(InputAction.CallbackContext context)
    {   // only trigger if the mouse is first pressed down
        if (!context.started) return;
        
        HandleInput();
    }

    void HandleInput()
    {
        // get mouse pos
        Vector2 mousePos = Pointer.current.position.ReadValue();
        // convert screen pixel pos to world unit pos
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
        Vector2 clickPoint = new Vector2(worldPos.x, worldPos.y);

        // check mouse and joint overlap
        Collider2D hit = Physics2D.OverlapPoint(clickPoint);

        if (hit != null && hit.CompareTag("Joint"))
        {
            SelectedJoint(hit.gameObject);
        }
        else
        {
            SpawnJoint(clickPoint);
        }
    }
    void SpawnJoint(Vector2 pos)
    {
        GameObject newJoint = Instantiate(jointPrefab, pos, Quaternion.identity);
        newJoint.name = $"Joint_{currentStructure.joints.Count}";

        // link to joint class
        Joint data = new Joint(currentStructure.joints.Count, pos);
        currentStructure.joints.Add(data);
    }

    void SelectedJoint(GameObject jointObj)
    {
        // logic for first select vs second select
        if (selectedJointA == null)
        {
            selectedJointA = jointObj;
            jointObj.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else if (selectedJointA == jointObj)
        {
            // deselect if clicking the same joint twice
            selectedJointA.GetComponent<SpriteRenderer>().color = Color.white;
            selectedJointA = null;
        }
        else
        {
            // connect selectedJointA to jointObj
        }
    }
}

