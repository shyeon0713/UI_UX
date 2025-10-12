using System;
using UnityEngine;

public static class PhotoPickerService
{
    public static void PickMany(Action<string[]> onPicked, string title = "사진 선택")
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        // 3-인자: callback, title, mime
        NativeGallery.GetImagesFromGallery(
            paths => onPicked?.Invoke(paths),
            title,
            "image/*"
        );
#else
        onPicked?.Invoke(null);
#endif
    }
}

