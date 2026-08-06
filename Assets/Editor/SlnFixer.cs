using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

internal class SlnFixer : AssetPostprocessor
{
    private static readonly Regex PlayerProjectRegex = new(
        @"=\s*""([^""]+)("",\s*""[^""]+\.Player\.csproj"")",
        RegexOptions.Compiled);

    private static bool _slrScheduled;

    private static string OnGeneratedSlnSolution(string path, string content)
    {
        return FixContent(content);
    }

    private static string OnGeneratedCSProject(string path, string content)
    {
        if (!_slrScheduled)
        {
            _slrScheduled = true;
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    TryFixSlnFile();
                    _slrScheduled = false;
                };
            };
        }
        return content;
    }

    private static void TryFixSlnFile()
    {
        var root = Path.GetDirectoryName(Application.dataPath);
        if (root == null) return;
        var slnPath = Path.Combine(root, "Weird-Factory.sln");
        if (!File.Exists(slnPath)) return;

        var content = File.ReadAllText(slnPath);
        var fixedContent = FixContent(content);
        if (fixedContent != content)
            File.WriteAllText(slnPath, fixedContent);
    }

    private static string FixContent(string content)
    {
        return PlayerProjectRegex.Replace(content, match =>
        {
            var name = match.Groups[1].Value;
            if (name.EndsWith(".Player"))
                return match.Value;
            return $"= \"{name}.Player{match.Groups[2].Value}";
        });
    }
}
