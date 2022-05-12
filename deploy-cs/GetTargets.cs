using Newtonsoft.Json;

namespace deploy_cs;

public class GetTargets
{
    internal static Targets AcquireTargets(string Target)
    {
        Target += "/targets.json";
        if (!File.Exists(Target))
        {
            Console.WriteLine("No targets.json found.");
            GenerateConfig();
            Environment.Exit(1);
            return null;
        } else {
            string json = File.ReadAllText(Target);
            Targets t = JsonConvert.DeserializeObject<Targets>(json);
            return t;
        };
    }
    internal static string GetFlakeDir()
    {
        string flakeDir = "";
        if (File.Exists("targets.json"))
        {
            flakeDir = Directory.GetCurrentDirectory();
        }
        else if (File.Exists("/etc/nixos/targets.json"))
        {
            flakeDir = "/etc/nixos/"; 
        }

        if (flakeDir == "")
        {
            GenerateConfig();
            Environment.Exit(1);
        }

        return flakeDir;
    }

    private static void GenerateConfig()
    {
        Console.WriteLine("Creating an example entry and exiting. It'll be in /etc/nixos/targets.json. Please edit it!");
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
        File.WriteAllText("/etc/nixos/targets.json" ,JsonConvert.SerializeObject(t, Formatting.Indented));
    }
}