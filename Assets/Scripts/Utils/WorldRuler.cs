using UnityEngine;
using TMPro;

public class WorldRuler : MonoBehaviour
{
    public GameObject markerPrefab;
    public int markerInterval = 5; // default to every 5 meteres
    public int initialDistance = 100; // How far out to generate initially
    public float floorY = -7f; // set it to be just above the floor

    void Start()
    {
        GenerateMarkers();
    }

    void GenerateMarkers()
    {
        for (int i = 0; i < initialDistance; i += markerInterval)
        {
            float xPos = i * markerInterval;
            GameObject marker = Instantiate(markerPrefab, new Vector3(xPos, floorY, 0), Quaternion.identity, transform);
            
            // set the text to the distance
            TMP_Text text = marker.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = i.ToString() + "m";
        }
    }
}