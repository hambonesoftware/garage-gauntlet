using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GarageGauntlet.Editor
{
    public static class BuildRailwayRush
    {
        [MenuItem("Garage Gauntlet/Build GitHub Pages site")]
        public static void BuildWeb()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("Add Web Build Support to this Unity Editor in Unity Hub.");

            PlayerSettings.productName = "Garage Gauntlet: Railway Rush";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "PROJECT:RailwayRush";

            string destination = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Web"));
            string[] args = Environment.GetCommandLineArgs();
            int output = Array.IndexOf(args, "-raceOutput");
            if (output >= 0)
            {
                if (output + 1 >= args.Length || args[output + 1].StartsWith("-", StringComparison.Ordinal))
                    throw new ArgumentException("-raceOutput requires a directory.");
                destination = Path.GetFullPath(args[output + 1]);
            }

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/RailwayRush.unity" },
                locationPathName = destination,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Railway Rush Web build " + report.summary.result);
            File.WriteAllText(Path.Combine(destination, ".nojekyll"), string.Empty);
        }
    }
}
