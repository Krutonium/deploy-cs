using System.Diagnostics;
using Newtonsoft.Json;

namespace deploy_cs // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        //https://nixos.wiki/wiki/Flakes
        static void Main(string[] args)
        {
            Program p = new Program();
            p.BetterMain(args);
        }

        private void BetterMain(string[] args)
        {
            string directory = GetFlakeDir();
            if (!File.Exists(directory + "./targets.json"))
            {
                Console.WriteLine("No targets.json found in " + directory);
                Console.WriteLine("Creating an example entry and exiting. Please edit it!");
                Targets t = new Targets
                {
                    Devices = new List<Device>()
                };
                Device d = new Device
                {
                    Name = "Example Device",
                    Ip = "192.168.0.1",
                    User = "root",
                    Comment = "This is an example device. This comment field isn't actually used in the program, but is included for your convenience."
                };
                t.Devices.Add(d);
                File.WriteAllText(directory + "./targets.json" ,JsonConvert.SerializeObject(t, Formatting.Indented));
                Environment.Exit(1);
            }
            UpdateFlake(directory);

            var devices = JsonConvert.DeserializeObject<Targets>(File.ReadAllText(directory + "./targets.json"));
            if (devices.ParallelDeploy)
            {
                foreach(var device in devices.Devices)
                {
                    Console.WriteLine("Deplying to all devices");
                    Console.Title = "Mass Deployment in Progress";
                    List<Task> tasks = new List<Task>();
                    foreach (var d in devices.Devices)
                    {
                        tasks.Add(new Task (() => Deploy(directory, device, devices.BuildHost, devices.BuildHostEnabled, true)));
                    }

                    foreach (var task in tasks)
                    {
                        task.Start();
                    }
                    Task.WaitAll(tasks.ToArray());
                    Console.WriteLine("Deployment complete");
                }
            }
            else
            {
                foreach (var device in devices.Devices)
                {
                    Console.WriteLine("Deploying to " + device.Name);
                    Console.Title = device.Name;
                    Deploy(directory, device, devices.BuildHost, devices.BuildHostEnabled);
                }
            }
        }
        private string GetFlakeDir()
        {
            string flakeDir = "";
            if (File.Exists("flake.nix"))
            {
                flakeDir = Directory.GetCurrentDirectory();
            }
            else
            {
                if (File.Exists("/etc/nixos/flake.nix"))
                {
                    flakeDir = "/etc/nixos/";
                }
            }

            if (flakeDir == "")
            {
                Console.WriteLine("Could not find flake repository.");
                Environment.Exit(1);
            }

            return flakeDir;
        }
        private void Deploy(string directory, Device d, Device buildHost, bool buildHostEnabled, bool quiet_unless_error = false) 
        {
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
        }
        private void GitPull(string directory)
        {
            GitAction(directory, "pull");
        }
        private void GitPush(string directory)
        {
            GitAction(directory, "add .");
            GitAction(directory, "commit -m \"deploy\"");
            GitAction(directory, "push");
        }
        private void GitAction(string directory, string action)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "git";
            startInfo.Arguments = action;
            startInfo.WorkingDirectory = directory;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }
        private void UpdateFlake(string directory)
        {
            GitPull(directory);
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "nix";
            psi.Arguments = "flake update --commit-lock-file";
            psi.WorkingDirectory = directory;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                Console.WriteLine("Error: " + p.StandardError.ReadToEnd());
            }
            GitPush(directory);
        }
    }
}