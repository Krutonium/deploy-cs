using System.Diagnostics;
using Newtonsoft.Json;

namespace deploy_cs // Note: actual namespace depends on the project name.
{
    
    
    //TODO:
    // - Build all configurations at the same time using
    // nixos-rebuild --flake .#device1 --flake .#device2 --no-link --buildHosts build
    // Experiment with perhaps merging multiple deployments into one?


    internal class Program
    {
        //https://nixos.wiki/wiki/Flakes
        static void Main(string[] args)
        {
            Console.WriteLine("Running Version Basically Balogna");
            Program p = new Program();
            p.BetterMain(args);
        }

        private void BetterMain(string[] args)
        {
            string directory = GetTargets.GetFlakeDir();
            UpdateFlake(directory);
            var devices = GetTargets.AcquireTargets(directory);
            var onlineDevices = new targetCheck().GetOnlineDevices(devices.Devices);
            foreach (var device in onlineDevices)
            {
                Console.Title = device.Name;
                new Deploy().DoDeploy(directory, device);
            }
            Console.WriteLine("Program Complete");
            Console.Beep();
            Environment.Exit(0);
        }



        private void UpdateFlake(string directory)
        {
            
            GitSupport.GitPull(directory);
            
            //Quickly Format
            ProcessStartInfo format = new ProcessStartInfo
            {
                FileName = "nixpkgs-format",
                Arguments = ".",
                WorkingDirectory = directory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process formatProcess = Process.Start(format);
            formatProcess.WaitForExit();
            
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "nix",
                Arguments = "flake update --commit-lock-file",
                WorkingDirectory = directory,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            GitSupport.GitPush(directory);
        }
    }
}