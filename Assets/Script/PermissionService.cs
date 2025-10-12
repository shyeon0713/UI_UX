using UnityEngine;

public class PermissionService
{
#if UNITY_ANDROID && !UNITY_EDITOR  // Android 디바이스일 경우 빌드
    static bool Has(string p) => Android.Permission.HasUserAuthorizedPermission(p);
    static void Req(string p) => Android.Permission.RequestUserPermission(p);
    static int Sdk() => new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
#endif
    public static void EnsureForGalleryAndExif()
    {
#if UNITY_ANDROID && !UNITY_EDITOR  // Android 디바이스일 경우 빌드
        if (Sdk() >= 33) { if (!Has("android.permission.READ_MEDIA_IMAGES")) Req("android.permission.READ_MEDIA_IMAGES"); }
        else { if (!Has("android.permission.READ_EXTERNAL_STORAGE")) Req("android.permission.READ_EXTERNAL_STORAGE"); }
        if (!Has("android.permission.ACCESS_MEDIA_LOCATION")) Req("android.permission.ACCESS_MEDIA_LOCATION");
#endif
    }
}
