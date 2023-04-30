using System.Text.Json;

namespace deploy;

partial class Program
{
    private static Config? ReadConfig()
    {
        Config? cfg = new Config();
        if (File.Exists(ConfigPath))
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.IncludeFields = true;
            cfg = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath), options);
        }
        else
        {
            cfg._machines = new List<Machine>();
            var m = new Machine
            {
                Name = "Example Machine",
                Comment = "This is an example machine",
                User = "root",
                Ip = "1.1.1.1"
            };
            cfg._machines.Add(m);
            cfg.MaxParallel = 1;
            JsonSerializerOptions? options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.IncludeFields = true;
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, options));
            _doNotContinue = true;
        }

        return cfg;
    }

    public class Config
    {
        public int MaxParallel;
        public List<Machine> _machines = new List<Machine>();
    }

    public class Machine
    {
        public string Name;
        public string User;
        public string Ip;
        public string Comment;
    }
}