using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch entry points for the test project. Run with -executeMethod.
// Sets iOS and Android player settings, then builds an empty scene per platform
// to prove the toolchains work before the locative package is imported.
public static class CIPlatformSetup
{
    const string BundleId = "com.companionintelligence.worldscalexrunity";
    const string ScenePath = "Assets/Scenes/Main.unity";

    public static void ConfigurePlayers()
    {
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.iOS.cameraUsageDescription = "Camera is used to show the augmented reality view.";
        PlayerSettings.iOS.locationUsageDescription = "Location is used to place augmented content at real-world positions.";

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        EnsureScene();
        AssetDatabase.SaveAssets();
        Debug.Log("CIPLATFORM player settings configured for iOS and Android");
    }

    public static void BuildIOS()
    {
        EnsureScene();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        Build(BuildTarget.iOS, Path.Combine(ProjectRoot(), "Builds", "iOS"));
    }

    public static void BuildAndroid()
    {
        EnsureScene();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        Build(BuildTarget.Android, Path.Combine(ProjectRoot(), "Builds", "Android", "Worldscale-XR-Unity.apk"));
    }

    static void EnsureScene()
    {
        if (!File.Exists(Path.Combine(ProjectRoot(), ScenePath)))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    static void Build(BuildTarget target, string location)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(location));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = location,
            target = target,
            options = BuildOptions.Development,
        });
        var summary = report.summary;
        Debug.Log($"CIPLATFORM build {target}: {summary.result}, errors={summary.totalErrors}, size={summary.totalSize}, output={location}");
        if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }

    static string ProjectRoot()
    {
        return Directory.GetParent(Application.dataPath).FullName;
    }
}
