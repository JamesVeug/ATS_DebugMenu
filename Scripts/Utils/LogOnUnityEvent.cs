using System;
using ATS_API.Helpers;
using DebugMenu;
using UnityEngine;

public class LogOnUnityEvent : MonoBehaviour
{
    private void OnEnable()
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} OnEnable\n{Environment.StackTrace}");
    }

    private void OnDisable()
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} OnDisable\n{Environment.StackTrace}");
    }

    private void Awake()
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} Awake\n{Environment.StackTrace}");
    }

    private void Start()
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} Start\n{Environment.StackTrace}");
    }

    private void OnDestroy()
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} OnDestroy\n{Environment.StackTrace}");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} OnApplicationFocus:{hasFocus}\n{Environment.StackTrace}");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} OnApplicationPause:{pauseStatus}\n{Environment.StackTrace}");
    }

    private void OnApplicationQuit()
    {
        Plugin.Log.LogInfo($"{gameObject.FullName()} OnApplicationQuit\n{Environment.StackTrace}");
    }
}