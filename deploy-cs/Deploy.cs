using System.Diagnostics;

namespace deploy_cs;

internal class Deploy
{
    internal void DoDeploy(string directory, Device d, List<Device> buildHosts, bool buildHostEnabled) 
    {
        Console.WriteLine("Deploying to {0}", d.Name);

        string BuildHostString = "";
        foreach (var bh in buildHosts)
        {
            BuildHostString += $"--build-host {bh.User}@{bh.Ip} ";
        }
        
        string arg = "";
        if (buildHostEnabled)
        {
            arg = $"--flake .#{d.Name} --target-host {d.User}@{d.Ip} {BuildHostString} switch --use-remote-sudo";
        }
        else
        {
            arg = $"--flake .#{d.Name} --target-host {d.User}@{d.Ip} switch --use-remote-sudo";
        }

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
        p.OutputDataReceived += (sender, e) => Console.WriteLine($"[{d.Name}] {e.Data}");
        p.ErrorDataReceived += (sender, e) => Console.WriteLine($"[{d.Name}] {e.Data}");
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();
        Console.WriteLine("Deploy Complete on {0}", d.Name);
    }
}