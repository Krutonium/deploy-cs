using System.Diagnostics;

namespace deploy_cs;

internal class Deploy
{
    internal void DoDeploy(string directory, List<Device> targetHosts, List<Device> buildHosts, bool buildHostEnabled) 
    {
        Console.WriteLine("Deploying to " + targetHosts.Count + " devices");

        string BuildHostString = "";
        if(buildHostEnabled)
        {
            Console.WriteLine("Building on " + buildHosts.Count + " devices");
            foreach (var bh in buildHosts)
            {
                BuildHostString += $"--build-host {bh.User}@{bh.Ip} ";
            }
        }

        string TargetHostString = "";
        foreach (var th in targetHosts)
        {
            TargetHostString += $"--flake .#{th.Name} --target-host {th.User}@{th.Ip} ";
        }
        
        string arg = $"{TargetHostString} {BuildHostString} switch --use-remote-sudo --log-format bar-with-logs -Q";
        

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
        p.OutputDataReceived += (sender, e) => Console.WriteLine($"{e.Data}");
        p.ErrorDataReceived += (sender, e) => Console.WriteLine($"{e.Data}");
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        p.WaitForExit();
        p.Close();
        Console.WriteLine("Deployment complete");
    }
}