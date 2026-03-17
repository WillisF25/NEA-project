using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the simulation stats panel UI.
/// Shows all time best, current gen best, and followed creature stats.
/// Can be toggled visible/hidden.
/// </summary>
public class StatsPanel : MonoBehaviour
{
    [Header("Manager Reference")]
    public SimulationManager simManager;

    [Header("Panel Root")]
    public GameObject panelRoot;

    [Header("Text Fields")]
    public TMP_Text allTimeBestText;
    public TMP_Text genBestText;
    public TMP_Text genNumberText;
    public TMP_Text creatureIDText;
    public TMP_Text speciesIDText;
    public TMP_Text nodeCountText;
    public TMP_Text connectionCountText;
    public TMP_Text currentDistText;

    [Header("Toggle Button")]
    public TMP_Text toggleButtonText;

    private bool isPanelVisible = true;

    void Update()
    {
        if (simManager == null) return;
        UpdateTexts();
    }

    void UpdateTexts()
    {
        // Global stats
        if (allTimeBestText != null)
            allTimeBestText.text = $"All-Time Best: {simManager.allTimeHigh:F2}m";

        if (genBestText != null)
            genBestText.text = $"Gen Best: {simManager.currentGenBest:F2}m";

        if (genNumberText != null)
            genNumberText.text = $"Generation: {simManager.neatSystem.generationNumber}";

        // Followed creature stats
        CreatureFollower best = simManager.currentBestCreature;

        if (best == null || best.assignedGenome == null)
        {
            if (creatureIDText != null)    creatureIDText.text    = "Genome ID: -";
            if (speciesIDText != null)     speciesIDText.text     = "Species: -";
            if (nodeCountText != null)     nodeCountText.text     = "Nodes: -";
            if (connectionCountText != null) connectionCountText.text = "Connections: -";
            if (currentDistText != null)   currentDistText.text   = "Distance: -";
            return;
        }

        Genome g = best.assignedGenome;

        if (creatureIDText != null)
            creatureIDText.text = $"Genome ID: {g.genomeID}";

        if (speciesIDText != null)
            speciesIDText.text = $"Species: {g.speciesID}";

        if (nodeCountText != null)
            nodeCountText.text = $"Nodes: {g.nodes.Count}";

        if (connectionCountText != null)
            connectionCountText.text = $"Connections: {g.connections.Count}";

        if (currentDistText != null)
            currentDistText.text = $"Distance: {best.leadingJoint.position.x:F2}m";
    }

    /// <summary>Called by the toggle button's OnClick event.</summary>
    public void TogglePanel()
    {
        isPanelVisible = !isPanelVisible;
        panelRoot.SetActive(isPanelVisible);
        toggleButtonText.text = isPanelVisible ? "Stats ▲" : "Stats ▼";
    }
}