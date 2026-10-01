using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Compila el juego para Windows (64 bits) en la carpeta Ejecutable/ (en la raíz del repositorio).
/// Desde Unity: menú "Snake > Compilar para Windows".
/// Desde la consola: Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWindows
/// </summary>
public static class BuildScript
{
    private const string OutputFolder = "Ejecutable";
    private const string ExeName = "Snake.exe";

    [MenuItem("Snake/Compilar para Windows")]
    public static void BuildWindows()
    {
        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = Path.Combine(OutputFolder, ExeName),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Compilación exitosa: {summary.outputPath} ({summary.totalSize / (1024f * 1024f):F1} MB)");
            if (!Application.isBatchMode)
                EditorUtility.RevealInFinder(summary.outputPath);
        }
        else
        {
            Debug.LogError($"La compilación falló ({summary.result}) con {summary.totalErrors} errores.");
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }
}
