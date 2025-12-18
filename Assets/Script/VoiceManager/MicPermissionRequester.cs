using UnityEngine;

public class MicPermissionRequester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[PERMISSION] EnsureMicPermission called at app start");
        MicPermissionUtil.EnsureMicPermission();
    }
}
