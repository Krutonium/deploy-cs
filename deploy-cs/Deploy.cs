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
            TargetHostString += $"--flake .#{th.Name} --target-host {th.User}@{th.Ip} {BuildHostString} ";
        }
        
        string arg = $"{TargetHostString} switch --use-remote-sudo";
        

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
        
        Console.WriteLine("Command to be executed:");
        Console.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");
        Console.WriteLine("---");
        Console.WriteLine(TargetHostString);
        
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