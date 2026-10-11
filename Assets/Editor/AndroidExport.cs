#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidExport
{
    [MenuItem("Tools/Android/Build Test APK")]
    public static void Export()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
        {
            EditorUtility.DisplayDialog("缺少 Android 建置工具", "請在 Unity Hub 為 Unity 2022.3.62f3 安裝 Android Build Support、Android SDK & NDK Tools 與 OpenJDK。", "確定");
            return;
        }
        string file = EditorUtility.SaveFilePanel("輸出手機測試 APK", Path.GetFullPath("Builds"), "MarineEncyclopedia", "apk");
        if (!string.IsNullOrEmpty(file)) ExportTo(file);
    }

    public static void ExportBatch()
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-apkOutput");
        string path = index >= 0 && index + 1 < args.Length ? args[index + 1] : "Builds/Android/MarineEncyclopedia.apk";
        ExportTo(Path.GetFullPath(path));
    }

    private static void ExportTo(string path)
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new BuildFailedException("Install Android Build Support, SDK & NDK Tools and OpenJDK first.");
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new BuildFailedException("Stop Play mode before building the APK.");
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0 || scenes.Any(scene => !File.Exists(scene)))
            throw new BuildFailedException("An enabled build scene is missing.");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
        PlayerSettings.Android.useCustomKeystore = false; // Local test APK uses the debug signing key.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        if (string.IsNullOrEmpty(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)))
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Standalone));
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.Android,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("APK build failed: " + report.summary.result);
        Debug.Log("Android test APK: " + path);
    }
}
#endif
