using UnityEditor;
using UnityEngine;

namespace BiteCheck.Editor
{
    public static class AndroidBuildConfigurator
    {
        [MenuItem("Tools/Bite Check/Apply Android Portrait Settings")]
        public static void ApplyAndroidPortraitSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.zozi.bitecheck");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.allowDebugging = true;

            AssetDatabase.SaveAssets();
            Debug.Log("Bite Check Android portrait settings applied. Confirm scenes and keystore settings in Build Settings before building.");
        }
    }
}
