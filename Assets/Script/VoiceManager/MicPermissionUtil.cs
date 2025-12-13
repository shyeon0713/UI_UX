using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public static class MicPermissionUtil
{
    public static bool EnsureMicPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            return false; // ¾ÆÁ÷ ½ÂÀÎ Àü
        }
#endif
        return true; // ÀÌ¹Ì ½ÂÀÎµÊ(¶Ç´Â Android ¾Æ´Ô)
    }
}
