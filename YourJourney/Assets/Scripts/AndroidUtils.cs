using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;

public class AndroidUtils
{
    public static void requestAndroidPermissions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
        }
#endif
    }

    /// <summary>
    /// Return the base path for Android documents, or the provided path if not on Android
    /// </summary>
    public static string androidBasePathOr(string mydocs)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
        {
            mydocs = "/storage/emulated/0/Documents";
        }
#endif
        return mydocs;
    }
}
