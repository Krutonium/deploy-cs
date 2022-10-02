using System.Diagnostics;

namespace deploy_cs;

internal class Deploy
{
    internal void DoDeploy(string directory, Device Target) 
    {
        Console.WriteLine($"Deploying to {Target.Name}");
        Console.Title = $"Deploying to {Target.Name}";
        string TargetHostString = "";
        TargetHostString += $"--flake .#{Target.Name} --target-host {Target.User}@{Target.Ip} switch ";
        string arg = $"{TargetHostString} --use-remote-sudo";

        ProcessStartInfo startInfo = new()
        {
            FileName = "nixos-rebuild",
            Arguments = arg,
            WorkingDirectory = directory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process p = new Process() { StartInfo = startInfo };
        p.OutputDataReceived += (sender, e) => Console.WriteLine($"{Target.Name}: {e.Data}");
        p.ErrorDataReceived += (sender, e) => Console.WriteLine($"{Target.Name}: {e.Data}");
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();
        p.Close();
        Console.WriteLine("Deployment complete");
    }
}