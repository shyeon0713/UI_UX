using UnityEngine;
using System;

public class VoiceTest : MonoBehaviour
{
    AndroidJavaObject plugin;
    public static Action<string> OnSpeechResult;

   // void Awake()
 //   {
        // 씬 전환/패널 전환에 사라지면 메시지 못 받을 수 있으니 필요하면 유지
        // DontDestroyOnLoad(gameObject);
   //}

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                plugin = new AndroidJavaObject("com.example.voicerecognition.VoicePlugin", activity);
                Debug.Log("[VoiceTest] plugin created OK");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[VoiceTest] plugin init failed: " + e);
        }
#endif
    }

    public void StartVoice()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    Debug.Log("[VoiceTest] StartVoice CALLED");

    if (plugin == null)
    {
        Debug.LogError("[VoiceTest] plugin is null");
        return;
    }

    Debug.Log("[VoiceTest] Calling startVoiceRecognition()");
    plugin.Call("startVoiceRecognition");
#endif
    }

    public void OnSpeechResultFromAndroid(string result)
    {
        Debug.Log("[VoiceResult] " + result);
        OnSpeechResult?.Invoke(result);
    }
}
