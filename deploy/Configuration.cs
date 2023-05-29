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
                Verb = "switch",
                Ip = "1.1.1.1"
            };
            cfg._machines.Add(m);
            cfg.MaxParallel = 1;
            cfg.Path_Private_SSH_Key = "/full/path/to/.ssh/id_ed25519";
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
        public bool Update_Flake = true;
        public string Path_Private_SSH_Key = "This will be used by default unless a different one is defined.";
        public List<Machine> _machines = new List<Machine>();
    }

    public class Machine
    {
        public string Name;
        public string User;
        public string Ip;
        public string Verb;
        public string Path_Private_SSH_Key_If_Not_Same;
        public string Comment;
    }
}