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
            if (devices.ParallelDeploy)
            {

                Console.WriteLine("Deploying to all devices");
                Console.Title = "Mass Deployment in Progress";  
                List<Task> tasks = new List<Task>();
                foreach (var d in devices.Devices)
                {
                    var Task = new Task(() => new Deploy().DoDeploy(directory, d, devices.BuildHost, devices.BuildHostEnabled, quiet_unless_error:false));
                    System.Threading.Thread.Sleep(1000);
                    Task.Start();
                    tasks.Add(Task);
                }
                Task.WaitAll(tasks.ToArray());
                Console.WriteLine("Deployment complete");
            }
            else
            {
                foreach (var device in devices.Devices)
                {
                    Console.Title = device.Name;
                    new Deploy().DoDeploy(directory, device, devices.BuildHost, devices.BuildHostEnabled);
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