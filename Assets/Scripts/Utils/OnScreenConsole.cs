using UnityEngine;
using TMPro;

/// <summary>
/// Captures Unity's internal debug logs and redirects them to an on-screen UI element.
/// Useful for tracking simulation events and errors.
/// </summary>
public class OnScreenConsole : MonoBehaviour
{
    public TextMeshProUGUI logDisplay;
    public int maxLines = 3;
    private System.Collections.Generic.List<string> logQueue = new System.Collections.Generic.List<string>();

    /// <summary>
    /// Subscribes to Unity's global log event when the object is active.
    /// </summary>
    void OnEnable()
    {
        // Ttells Unity to call the function every time a Log happens
        Application.logMessageReceived += HandleLog;
    }

    /// <summary>
    /// Unsubscribes from the log event to prevent memory leaks and errors when the UI is disabled.
    /// </summary>
    void OnDisable()
    {
        // stop listening when the object is destroyed
        Application.logMessageReceived -= HandleLog;
    }

    /// <summary>
    /// Processes the incoming log message, colors it based on severity, and updates the UI.
    /// </summary>
    /// <param name="logString">The actual message sent to the console.</param>
    /// <param name="stackTrace">Details on where the code call came from (ignored here for brevity).</param>
    /// <param name="type">The severity: Log, Warning, Error, or Exception.</param>
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // add color based on log type
        string color = "white";
        if (type == LogType.Error || type == LogType.Exception) color = "red";
        if (type == LogType.Warning) color = "yellow";

        string formattedLog = $"<color={color}>[{type}] {logString}</color>";
        
        logQueue.Add(formattedLog);

        // remove old lines if we exceed the limit
        if (logQueue.Count > maxLines)
        {
            logQueue.RemoveAt(0);
        }

        // display it
        if (logDisplay != null)
        {
            logDisplay.text = string.Join("\n", logQueue);
        }
    }
}