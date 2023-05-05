using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace deploy 
{
    partial class Program
    {
        private const string ConfigPath = "config.json";
        private static bool _doNotContinue = false;
        private static ParallelOptions _parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = 1};
        static void Main(string[] args)
        {
            var config = ReadConfig();
            if (_doNotContinue || config._machines[0].Ip == "1.1.1.1")
            {
                string path = Path.GetFullPath(ConfigPath);
                Console.WriteLine("Please edit {0} and re-run the application", path);
                Environment.Exit(1);
            }

            if (config.Update_Flake)
            {
                Console.WriteLine("Updating Flake Lockfile");
                Process.Start("nix", "flake update --commit-lock-file").WaitForExit();
                
            }
            git.gitSync(".");
            _parallelOptions.MaxDegreeOfParallelism = config.MaxParallel;
            var onlineDevices = OnlineDevices(config);
            Console.WriteLine("Online Devices: ");
            foreach (var dev in onlineDevices)
            {
                Console.WriteLine(dev.Name);
            }
            //Update Flake Lock if Enabled


            //Build Derivations
            Console.WriteLine("Building Derivations");
            var builtDevices = new List<Machine>();
            foreach (var device in onlineDevices)
            {
                Console.Title = $"Building {device.Name}";
                Console.WriteLine("Building {0}", device.Name);
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "nix";
                string tempPath = $"{Path.GetTempPath()}/{device.Name}";
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                psi.Arguments = $"build .#nixosConfigurations.{device.Name}.config.system.build.toplevel " +
                                $"--out-link {tempPath}";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WorkingDirectory = Path.GetFullPath(".");
                Process process = new Process();
                process.StartInfo = psi;
                process.OutputDataReceived +=
                    (sender, eventArgs) => Console.WriteLine($"{device.Name}: {eventArgs.Data}");
                process.ErrorDataReceived +=
                    (sender, eventArgs) => Console.WriteLine($"{device.Name}: {eventArgs.Data}");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                if (process is {ExitCode: 0})
                {
                    Console.WriteLine("Built {0}", device.Name);
                    builtDevices.Add(device);
                }
                else
                {
                    Console.WriteLine("Failed to build {0}", device.Name);
                }
                process.Close();
            }
            
            Console.WriteLine("Deploying to all online devices");
            Console.Title = "Deploying to all online devices";
            //Deploy in parallel to all online devices (up to CPU count, iirc).
            Parallel.ForEach(builtDevices, _parallelOptions, device =>
            {
                string tempPath = $"{Path.GetTempPath()}/{device.Name}";
                Console.WriteLine("Deploying to {0}", device.Name);
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "nix";
                psi.Arguments = $"copy --to ssh://{device.User}@{device.Ip} {tempPath}";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WorkingDirectory = Path.GetFullPath(".");
                Process process = new Process();
                process.StartInfo = psi;
                process.OutputDataReceived +=
                    (sender, eventArgs) => Console.WriteLine($"{device.Name}: {eventArgs.Data}");
                process.ErrorDataReceived +=
                    (sender, eventArgs) => Console.WriteLine($"{device.Name}: {eventArgs.Data}");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                psi.FileName = "ssh";
                psi.Arguments =
                    $"ssh {device.User}@{device.Ip} -- sudo {ReadLink(tempPath)}/bin/switch-to-configuration {device.Verb}";
                process.Start();
                process.WaitForExit();
                if (process is {ExitCode: 0})
                {
                    Console.WriteLine("Deployed to {0}", device.Name);
                }
                else
                {
                    Console.WriteLine("Failed to deploy to {0}", device.Name);
                }
                process.Close();
            });

        }


        public static string ReadLink(string path)
        {
            var proc = new Process 
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "readlink",
                    Arguments = "-f " + path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string output = "";
            while (!proc.StandardOutput.EndOfStream)
            {
                output += proc.StandardOutput.ReadLine();
            }

            return output;
        }
        
        public static List<Machine> OnlineDevices(Config config){
            Console.Title = "Checking for Online Devices...";
            Console.WriteLine("Checking for online devices");
            List<Machine> devices = new List<Machine>(); 
            List<Process> processes = new List<Process>();
            Parallel.ForEach(config._machines, _parallelOptions, device => { 
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "nix";
                psi.Arguments = $"store ping --store ssh://{device.Ip} --timeout 5";
                Process process = Process.Start(psi) ?? throw new InvalidOperationException();
                processes.Add(process);
                process?.WaitForExit(5000);
                if (process is {HasExited: true})
                {
                    if (process is {ExitCode: 0})
                    { devices.Add(device); };
                }
                else
                {
                    process?.Kill();
                }

            });
            return devices;
        }
    } 
}