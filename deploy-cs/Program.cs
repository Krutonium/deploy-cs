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
            Console.WriteLine("Running Version Apollo Astronaut");
            Program p = new Program();
            p.BetterMain(args);
        }

        private void BetterMain(string[] args)
        {
            string directory = GetTargets.GetFlakeDir();
            UpdateFlake(directory);
            var devices = GetTargets.AcquireTargets(directory);
            //Get list of Online Devices
            var onlineDevices = new targetCheck().GetOnlineDevices(devices.Devices);
            //Get list of Build Hosts
            var buildHosts = new targetCheck().GetBuildHosts(onlineDevices);
            if (devices.ParallelDeploy)
            {
                new Deploy().DoDeploy(directory, onlineDevices, buildHosts, devices.BuildHostEnabled);
            }
            else
            {
                foreach (var device in devices.Devices)
                {
                    Console.Title = device.Name;
                    List<Device> deployTo = new List<Device> {device};
                    new Deploy().DoDeploy(directory, deployTo, buildHosts, devices.BuildHostEnabled);
                }
            }
            Console.WriteLine("Program Complete");
            Console.Beep();
            Environment.Exit(0);
        }



        private void UpdateFlake(string directory)
        {
            GitSupport.GitPull(directory);
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "nix";
            psi.Arguments = "flake update --commit-lock-file";
            psi.WorkingDirectory = directory;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            GitSupport.GitPush(directory);
        }
    }
}