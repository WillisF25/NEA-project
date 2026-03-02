using UnityEngine;

/// <summary>
/// Updates the LinkRenderer visuals to stay aligned with the physcial joints.
/// </summary>
public class LinkFollower : MonoBehaviour 
{
    // the two joints
    public Transform startObj;
    public Transform endObj;
    
    private LineRenderer lr;

    /// <summary>
    /// Casches the LineRenderer component in the same GameObject.
    /// </summary>
    void Awake() 
    { 
        lr = GetComponent<LineRenderer>(); 
    }

    /// <summary>
    /// Assigns the physcial Transforms the line should track and forces an immediate visual update.
    /// </summary>
    /// <param name="start">The Transform of the source joint.</param>
    /// <param name="end">The Transform of the target joint.</param>
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

    /// <summary>
    /// Monitors if the joints still exitst and updates the line positions every frame.
    /// If a joint is destroyed, this visual object cleans itself to prevent memory leaks.
    /// </summary>
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
