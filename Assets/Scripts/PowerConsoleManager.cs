using UnityEngine;
using CI.PowerConsole;
using System;

public class PowerConsoleManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PowerConsole.Initialise();
        PowerConsole.LogLevel = LogLevel.Trace;
        Application.logMessageReceived += HandleLog;
    }

    void Start()
    {
        print("PowerConsole say's hello");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            togglePowerConsole();
         }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        PowerConsole.Log(LogLevel.Debug, logString);
     }

    void togglePowerConsole()
    {
        PowerConsole.IsVisible = !PowerConsole.IsVisible;
     }

}
