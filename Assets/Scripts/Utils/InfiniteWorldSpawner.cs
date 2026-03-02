using UnityEngine;
using TMPro;

/// <summary>
/// Monitors the furthest-reaching creature and procedurally generates 
/// floors and distance markers ahead of it.
/// </summary>
public class InfiniteWorldSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject markerPrefab;

    [Header("Settings")]
    public float floorWidth = 200f; // width of floor prefab
    public int markerInterval = 5; // every 5 meters
    public float spawnBuffer = 50f; // how far ahead to keep spawning
    
    [Header("Positions")]
    public float floorY = -8f;
    public float markerY = -7f;

    // track furthest
    private float furthestFloorX = -10f; 
    private int furthestMarkerX = -5;

    /// <summary>
    /// Checks every frame if the lead creature has moved far enough to require new terrain.
    /// </summary>
    void Update()
    {
        // use lead joint from SimulationManager
        if (SimulationManager.focusTarget == null) return;

        float leaderX = SimulationManager.focusTarget.position.x;

        // check if need more floors
        while (furthestFloorX < leaderX + spawnBuffer)
        {
            furthestFloorX += floorWidth;
            SpawnFloor(furthestFloorX);
        }

        // check if need more markers
        while (furthestMarkerX < leaderX + spawnBuffer)
        {
            furthestMarkerX += markerInterval;
            SpawnMarker(furthestMarkerX);
        }
    }
    
    /// <summary>
    /// Places a floor prefab at the specified x coordinate.
    /// </summary>
    /// <param name="x">The horizontal position to place the floor segment.</param>
    void SpawnFloor(float x)
    {
        Instantiate(floorPrefab, new Vector3(x, floorY, 0), Quaternion.identity, transform);
    }

    /// <summary>
    /// Places a marker prefab and updates its text component to show the distance.
    /// </summary>
    /// <param name="distance">The integer distance value (e.g., 50 for "50m").</param>
    void SpawnMarker(int distance)
    {
        GameObject marker = Instantiate(markerPrefab, new Vector3(distance, markerY, 0), Quaternion.identity, transform);
        
        TMP_Text text = marker.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = distance.ToString() + "m";
    }
}