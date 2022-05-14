using System.Diagnostics;

namespace deploy_cs;

internal class Deploy
{
    internal void DoDeploy(string directory, Device d, Device buildHost, bool buildHostEnabled, bool quiet_unless_error = false) 
    {
        Console.WriteLine("Checking if {0} is online", d.Name);
        bool online = new targetCheck().checkIfHostOnline(d.Ip);
        if (!online)
        {
            Console.WriteLine("{0} is offline", d.Name);
            return;
        }
        Console.WriteLine("Deploying to {0}", d.Name);
        string arg = "";
        if (buildHostEnabled)
        {
            arg = $"--flake .#{d.Name} --target-host {d.User}@{d.Ip} --build-host {buildHost.User}@{buildHost.Ip} switch --use-remote-sudo";
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
            CreateNoWindow = true
        };
        if (quiet_unless_error)
        {
            //Capture output and error, and if there's any error, print it.
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
        }
        Process p = new Process() { StartInfo = startInfo };
        p.Start();
        if (quiet_unless_error && false)
            //I can't make this work, because all of the output is in stderr for some reason
            //If you happen to know how to fix this, please let me know
        {
            string error = p.StandardOutput.ReadToEnd();
            if (error != "")
            {
                Console.WriteLine($"Errors in deployment on {d.Name}");
                Console.WriteLine(error);
            }
        }
        p.WaitForExit();
        Console.WriteLine("Deploy Complete on {0}", d.Name);
    }
}