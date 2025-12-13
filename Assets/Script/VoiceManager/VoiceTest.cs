using UnityEngine;

public class VoiceTest : MonoBehaviour
{
    AndroidJavaObject plugin;

    void Start()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            plugin = new AndroidJavaObject(
                "com.example.voicerecognition.VoicePlugin",
                activity
            );
        }
    }

    public void StartVoice()
    {
        plugin.Call("startVoiceRecognition");
    }
}
