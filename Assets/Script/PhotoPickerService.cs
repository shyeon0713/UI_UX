public static class PhotoPickerService
{
    public static void PickMany(System.Action<string[]> onPicked, int max = 300, string title = "Select photos")
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        // 1) 권한 체크 (bool)
        bool has = NativeGallery.CheckPermission(
            NativeGallery.PermissionType.Read,
            NativeGallery.MediaType.Image
        );

        if (!has)
        {
            // 2) 권한 요청 (비동기)
            NativeGallery.RequestPermissionAsync((perm) =>
            {
                if (perm != NativeGallery.Permission.Granted)
                {
                    onPicked?.Invoke(null); // 거부/취소
                    return;
                }
                // 3) 권한 OK → 픽커 호출
                DoPick(onPicked, max, title);
            },
            NativeGallery.PermissionType.Read,
            NativeGallery.MediaType.Image);

            return; // 콜백에서 이어짐
        }

        // 이미 권한 있음 → 바로 픽커
        DoPick(onPicked, max, title);
#else
        onPicked?.Invoke(null);
#endif
    }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    static void DoPick(System.Action<string[]> onPicked, int max, string title)
    {
        NativeGallery.GetImagesFromGallery(
            paths => onPicked?.Invoke(paths),
            title,
            "image/*",
            max
        );
    }
#endif
}
