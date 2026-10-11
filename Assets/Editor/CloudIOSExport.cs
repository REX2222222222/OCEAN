#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CloudIOSExport
{
    public static void Build()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
            throw new BuildFailedException("The cloud editor needs iOS Build Support.");

        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-iosOutput");
        string output = index >= 0 && index + 1 < args.Length ? args[index + 1] : "build/iOS";
        string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
            .Select(scene => scene.path).ToArray();
        if (scenes.Length == 0 || scenes.Any(scene => !File.Exists(scene)))
            throw new BuildFailedException("An enabled build scene is missing.");

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.rex2222222222.ocean");
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.iOS.cameraUsageDescription = "使用相機辨識海洋植物，並顯示對應的圖鑑資料。";
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        Directory.CreateDirectory(output);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("iOS export failed: " + report.summary.result);
        Debug.Log("Unsigned Xcode project exported: " + output);
    }
}
#endif
