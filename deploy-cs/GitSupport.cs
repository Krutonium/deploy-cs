using System.Diagnostics;

namespace deploy_cs;

internal static class GitSupport
{
    internal static void GitPull(string directory)
    {
        GitAction(directory, "pull");
    }
    internal static void GitPush(string directory)
    {
        GitAction(directory, "add .");
        GitAction(directory, "commit -m \"deploy\"");
        GitAction(directory, "push");
    }

    private static void GitAction(string directory, string action)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = action,
            WorkingDirectory = directory,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }
}