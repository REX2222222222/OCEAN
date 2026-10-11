#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class IOSExport
{
    [MenuItem("Tools/iOS/Export Xcode Project")]
    public static void Export()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
        {
            EditorUtility.DisplayDialog("缺少 iOS Build Support", "請在 Unity Hub 的 Unity 2022.3.62f3 安裝項目中加入 iOS Build Support，再匯出 Xcode 專案。", "確定");
            return;
        }
        string folder = EditorUtility.SaveFolderPanel("選擇 Xcode 專案輸出資料夾", Path.GetFullPath("Builds"), "iOS");
        if (string.IsNullOrEmpty(folder)) return;
        ExportTo(folder);
    }

    // For a Mac or cloud build host: -executeMethod IOSExport.ExportBatch -iosOutput <folder>
    public static void ExportBatch()
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-iosOutput");
        string output = index >= 0 && index + 1 < args.Length ? args[index + 1] : "Builds/iOS";
        ExportTo(Path.GetFullPath(output));
    }

    private static void ExportTo(string folder)
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
            throw new BuildFailedException("The Unity installation is missing iOS Build Support.");
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new BuildFailedException("Stop Play mode before exporting.");
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0 || scenes.Any(path => !File.Exists(path)))
            throw new BuildFailedException("An enabled build scene is missing.");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // ARM64
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        if (string.IsNullOrEmpty(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS)))
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Standalone));
        PlayerSettings.iOS.cameraUsageDescription = "使用相機辨識海洋植物，並顯示對應的圖鑑資料。";
        Directory.CreateDirectory(folder);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = folder,
            target = BuildTarget.iOS,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("iOS export failed: " + report.summary.result);
        Debug.Log("Xcode project exported to " + folder + ". Open it in Xcode on a Mac to sign and install on iPad.");
    }
}
#endif