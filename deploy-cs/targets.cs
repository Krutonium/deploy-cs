namespace deploy_cs;

public class Targets
{
    public List<Device> Devices;
    public Device BuildHost = new Device()
    {
        Name = "localhost",
        Ip = "127.0.0.1",
        User = "root",
        Comment = "This is the build host. It is not a target device, but it does need Nix to be running, with SSH enabled. If this is disabled, the local PC is used."
    };

    public bool BuildHostEnabled = false;
    public bool AddRoot = true;
    public bool ParallelDeploy = true;
}

public class Device
{
    public string Name;
    public string Ip;
    public string User;
    public string Comment;
}