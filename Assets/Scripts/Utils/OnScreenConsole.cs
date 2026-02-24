using UnityEngine;
using TMPro;

public class OnScreenConsole : MonoBehaviour
{
    public TextMeshProUGUI logDisplay;
    public int maxLines = 3;
    private System.Collections.Generic.List<string> logQueue = new System.Collections.Generic.List<string>();

    void OnEnable()
    {
        // Ttells Unity to call the function every time a Log happens
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // stop listening when the object is destroyed
        Application.logMessageReceived -= HandleLog;
    }

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