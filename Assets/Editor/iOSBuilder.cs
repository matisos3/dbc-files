using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class iOSBuilder
{
    public static void BuildiOS()
    {
        string targetDir = "build/iOS";

        // Ustawienie platformy docelowej na iOS
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

        // Pobranie listy scen zaznaczonych w Build Settings
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = targetDir;
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build iOS zakończony sukcesem: " + summary.totalSize + " bajtów");
        }
        else
        {
            Debug.LogError("Build iOS nie powiódł się: " + summary.result);
            EditorApplication.Exit(1);
        }
    }
}