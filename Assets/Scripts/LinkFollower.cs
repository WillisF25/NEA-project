using UnityEngine;

public class LinkFollower : MonoBehaviour 
{
    // the two joints
    public Transform startObj;
    public Transform endObj;
    
    private LineRenderer lr;

    void Awake() 
    { 
        lr = GetComponent<LineRenderer>(); 
    }

    public void SetTargets(Transform start, Transform end)
    {
        startObj = start;
        endObj = end;
        
        // update immediately to stop flicker
        if (lr != null)
        {
            lr.SetPosition(0, startObj.position);
            lr.SetPosition(1, endObj.position);
        }
    }

    void Update() 
    {
        // if joint deleted, delete link
        if (startObj == null || endObj == null) 
        {
            Destroy(gameObject);
            return;
        }

        // snap line endpoints
        lr.SetPosition(0, startObj.position);
        lr.SetPosition(1, endObj.position);
    }
}
