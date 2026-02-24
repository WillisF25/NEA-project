using UnityEngine;

public class BuilderSettingsManager : MonoBehaviour
{
    public static BuilderSettingsManager Instance;

    [Header("NEAT Settings")]
    public float mutateWeightRate = 0.8f;
    public float addNodeRate = 0.1f;
    public float addConnectionRate = 0.05f;
    public float compatibilityThreshold = 2.0f;
    public int populationLimit = 50;

    [Header("Simulation Settings")]
    public float generationTimeLimit = 20f;
    public float timeScale = 1.0f;
    public float oscillatorFreq = 2.0f;

    [Header("Muscle Physics")]
    public float muscleStrength = 50f;
    public float minLenMultiplier = 0.5f;
    public float maxLenMultiplier = 1.5f;

    void Awake()
    {
        // singleton pattern to keep settings accessible
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void ValidateAndFix()
    {
        // Logic Check: max len must be > min len
        float minLen = 0.5f; 
        float maxLen = 1.5f;
        if (maxLen <= minLen) maxLen = minLen + 0.1f;
        
        // Logic Check: population count
        if (populationLimit < 1) populationLimit = 1;
    }
}