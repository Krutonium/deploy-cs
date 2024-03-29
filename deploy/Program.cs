﻿using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Renci.SshNet;

namespace deploy 
{
    partial class Program
    {
        private const string ConfigPath = "config.json";
        private static bool _doNotContinue = false;
        private static ParallelOptions _parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = 1};
        private static Logger _logger = new Logger();
        
        static void Main(string[] args)
        {
            var config = ReadConfig();
            if (_doNotContinue || config._machines[0].Ip == "1.1.1.1")
            {
                string path = Path.GetFullPath(ConfigPath);
                Console.WriteLine("Please edit {0} and re-run the application", path);
                Environment.Exit(1);
            }
            _logger.path = config.LogLocation;
            if (config.Update_Flake)
            {
                //Update Flake Lock if Enabled
                Console.WriteLine("Updating Flake Lockfile");
                _logger.Log("Updating Flake Lockfile");
                Process.Start("nix", "flake update --commit-lock-file").WaitForExit();
            }
            git.gitSync(".");
            _parallelOptions.MaxDegreeOfParallelism = config.MaxParallel;
            _logger.Log("Parallelism set to " + config.MaxParallel);
            var onlineDevices = OnlineDevices(config);
            Console.WriteLine("Online Devices: ");
            foreach (var dev in onlineDevices)
            {
                Console.WriteLine(dev.Name);
                _logger.Log("Online Device: " + dev.Name);
            }
            
            //Build Derivations
            Console.WriteLine("Building Derivations");
            _logger.Log("Building Derivations");
            var builtDevices = new List<Machine>();
            foreach (var device in onlineDevices)
            {
                if (Build(device))
                {
                    _logger.Log("Built Derivation: " + device.Name);
                    builtDevices.Add(device);
                }
                else
                {
                    _logger.Log("Failed to build Derivation: " + device.Name);
                }
            }
            
            Console.WriteLine("Deploying to all online devices");
            _logger.Log("Deploying to all online devices");
            Console.Title = "Deploying to all online devices";
            //Deploy in parallel to all online devices (up to CPU count, iirc).
            Dictionary<string, bool> DeviceResults = new Dictionary<string, bool>();
            Parallel.ForEach(builtDevices, _parallelOptions, device =>
            {
                Program p = new Program();
                p.CopyToMachine(device);
                var result = p.Switch(device, config);
                DeviceResults.Add(device.Name, result);
            });
            Console.WriteLine("Deployed to all online devices");
            Console.WriteLine("Results:");
            string Success = "✅ Success!";
            String Failure = "❌ Failure!";
            ConsoleColor originalColor = Console.ForegroundColor;
            foreach (var device in DeviceResults)
            {
                switch (device.Value)
                {
                    case true:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"{device.Key}:");
                        Console.CursorLeft = Console.BufferWidth - Success.Length;
                        Console.WriteLine(Success);
                        Console.ForegroundColor = originalColor;
                        break;
                    case false:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write($"{device.Key}:");
                        Console.CursorLeft = Console.BufferWidth - Failure.Length;
                        Console.WriteLine(Failure);
                        Console.ForegroundColor = originalColor;
                        break;
                }
            }

            if (DeviceResults.ContainsValue(false))
            {
                Console.WriteLine("To diagnose issues, do the deploy manually to see output.");
            }
        }

        public static bool Build(Machine device)
        {
            Console.Title = $"Building {device.Name}";
            _logger.Log($"Building {device.Name}");
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
            process.OutputDataReceived += (sender, args) => _logger.Log($"{device.Name}: {args.Data}");
            process.ErrorDataReceived += (sender, args) => _logger.Log($"{device.Name}: {args.Data}");
            process.StartInfo = psi;
            process.Start();
            process.WaitForExit();
            bool Success = false;
            if (process is {ExitCode: 0})
            {
                Console.WriteLine("Built {0}", device.Name);
                _logger.Log($"Built {device.Name} Successfully");
                Success = true;
            }
            else
            {
                Console.WriteLine($"Failed to build {device.Name}");
            }
            process.Close();
            return Success;
        }

        public bool CopyToMachine(Machine device)
        {
            string tempPath = $"{Path.GetTempPath()}/{device.Name}";
            _logger.Log($"Copying {tempPath} to {device.Name}");
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
            process.OutputDataReceived += (sender, args) => _logger.Log($"{device.Name}: {args.Data}");
            process.ErrorDataReceived += (sender, args) => _logger.Log($"{device.Name}: {args.Data}");
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Console.WriteLine("Failed to copy to {0}", device.Name);
                return false;
            }

            return true;
        }

        public bool Switch(Machine device, Config config)
        {
            Console.WriteLine("Switching on {0}", device.Name);
            string tempPath = $"{Path.GetTempPath()}/{device.Name}";

            var privateKeyFile = new PrivateKeyFile(config.Path_Private_SSH_Key);
            var privateKeyAuth = new PrivateKeyAuthenticationMethod(device.User, privateKeyFile);
            
            var connectionInfo = new ConnectionInfo(device.Ip, device.User, privateKeyAuth);

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                if (client.IsConnected)
                {
                
                    var command = client.RunCommand($"sudo {ReadLink(tempPath)}/bin/switch-to-configuration {device.Verb}");
                    if(command.ExitStatus == 0)
                    {
                        Console.WriteLine("Switched successfully on {0}", device.Name);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to switch on {0}", device.Name);
                        return false;
                    }
                    client.Disconnect();
                }
                else
                {
                    Console.WriteLine("Failed to connect to {0}", device.Name);
                    return false;
                }
            }
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