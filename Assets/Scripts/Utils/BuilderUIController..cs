using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuilderUIController : MonoBehaviour
{
    [Header("NEAT Settings")]
    public Slider weightMutateSlider;
    public TMP_Text weightMutateText;

    public Slider addNodeSlider;
    public TMP_Text addNodeText;

    public Slider addConnectionSlider;
    public TMP_Text addConnectionText;
    
    public Slider compatThresholdSlider;
    public TMP_Text compatThresholdText;

    public Slider popLimitSlider;
    public TMP_Text popLimitText;

    [Header("Simulation Settings")]
    public Slider timeLimitSlider;
    public TMP_Text timeLimitText;

    public Slider timeScaleSlider;
    public TMP_Text timeScaleText;

    public Slider oscFreqSlider;
    public TMP_Text oscFreqText;

    [Header("Muscle Settings")]
    public Slider strengthSlider;
    public TMP_Text strengthText;
    public Slider minLenSlider;
    public TMP_Text minLenText;
    public Slider maxLenSlider;
    public TMP_Text maxLenText;

    void Start()
    {
        // set Sliders to match the current values in BuilderSettingsManager
        // ensure UI is in sync with code on start
        weightMutateSlider.value = BuilderSettingsManager.Instance.mutateWeightRate;
        addNodeSlider.value = BuilderSettingsManager.Instance.addNodeRate;
        addConnectionSlider.value = BuilderSettingsManager.Instance.addConnectionRate;
        compatThresholdSlider.value = BuilderSettingsManager.Instance.compatibilityThreshold;
        popLimitSlider.value = BuilderSettingsManager.Instance.populationLimit;

        timeLimitSlider.value = BuilderSettingsManager.Instance.generationTimeLimit;
        timeScaleSlider.value = BuilderSettingsManager.Instance.timeScale;
        oscFreqSlider.value = BuilderSettingsManager.Instance.oscillatorFreq;

        minLenSlider.value = BuilderSettingsManager.Instance.minLenMultiplier;
        maxLenSlider.value = BuilderSettingsManager.Instance.maxLenMultiplier;
        strengthSlider.value = BuilderSettingsManager.Instance.muscleStrength;

        // init label update
        UpdateAllLabels();
    }

    // neat functions
    public void OnWeightMutateChanged(float val)
    {
        BuilderSettingsManager.Instance.mutateWeightRate = val;
        weightMutateText.text = (val * 100f).ToString("F0") + "%"; 
    }

    public void OnAddNodeChanged(float val)
    {
        BuilderSettingsManager.Instance.addNodeRate = val;
        addNodeText.text = (val * 100f).ToString("F0") + "%"; 
    }

    public void OnAddConnectionChanged(float val)
    {
        BuilderSettingsManager.Instance.addConnectionRate = val;
        addConnectionText.text = (val * 100f).ToString("F0") + "%";
    }

    public void OnCompatThresholdChanged(float val)
    {
        BuilderSettingsManager.Instance.compatibilityThreshold = val;
        compatThresholdText.text = val.ToString("F1");
    }

    public void OnPopLimitChanged(float val)
    {  
        int intVal = Mathf.RoundToInt(val);
        BuilderSettingsManager.Instance.populationLimit = intVal;
        popLimitText.text = intVal.ToString();
    }

    // simulation functions

    public void OnTimeLimitChanged(float val)
    {
        BuilderSettingsManager.Instance.generationTimeLimit = val;
        timeLimitText.text = val.ToString("F0") + "s";
    }

    public void OnTimeScaleChanged(float val)
    {
        BuilderSettingsManager.Instance.timeScale = val;
        timeScaleText.text = val.ToString("F1") + "x";
    }

    public void OnOscFreqChanged(float val)
    {
        BuilderSettingsManager.Instance.oscillatorFreq = val;
        oscFreqText.text = val.ToString("F1") + "Hz";
    }

    // muscle funcions
    public void OnMuscleStrengthChanged(float val)
    {
        BuilderSettingsManager.Instance.muscleStrength = val;
        strengthText.text = val.ToString("F0");
    }

    public void OnMinLenChanged(float val)
    {
        BuilderSettingsManager.Instance.minLenMultiplier = val;
        minLenText.text = val.ToString("F2") + "x";
    }

    public void OnMaxLenChanged(float val)
    {
        BuilderSettingsManager.Instance.maxLenMultiplier = val;
        maxLenText.text = val.ToString("F2") + "x";
    }

    void UpdateAllLabels()
    {
        // refresh all labels
        OnWeightMutateChanged(weightMutateSlider.value);
        OnAddNodeChanged(addNodeSlider.value);
        OnAddConnectionChanged(addConnectionSlider.value);
        OnCompatThresholdChanged(compatThresholdSlider.value);
        OnPopLimitChanged(popLimitSlider.value);
        
        OnTimeLimitChanged(timeLimitSlider.value);
        OnTimeScaleChanged(timeScaleSlider.value);
        OnOscFreqChanged(oscFreqSlider.value);

        OnMuscleStrengthChanged(strengthSlider.value);
        OnMinLenChanged(minLenSlider.value);
        OnMaxLenChanged(maxLenSlider.value);
    }

        public void ExitApplication()
    {
        // only works in the actual .exe build
        Application.Quit();

        // testing in unity editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}