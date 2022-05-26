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
            string directory = GetTargets.GetFlakeDir();
            UpdateFlake(directory);
            var devices = GetTargets.AcquireTargets(directory);
            //Get list of Online Devices
            var onlineDevices = new targetCheck().GetOnlineDevices(devices.Devices);
            //Get list of Build Hosts
            var buildHosts = new targetCheck().GetBuildHosts(devices.Devices);
            if (devices.ParallelDeploy)
            {
                //Parallel.Foreach Deploy each Device
                Parallel.ForEach(onlineDevices, (device) =>
                {
                    new Deploy().DoDeploy(directory, device, buildHosts, devices.BuildHostEnabled);
                });
                Console.WriteLine("Deployment complete");
            }
            else
            {
                foreach (var device in devices.Devices)
                {
                    Console.Title = device.Name;
                    new Deploy().DoDeploy(directory, device, buildHosts, devices.BuildHostEnabled);
                }
            }
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