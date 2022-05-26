namespace deploy_cs;

public class Targets
{
    public List<Device> Devices;

    public bool BuildHostEnabled = false;
    public bool AddRoot = true;
    public bool ParallelDeploy = true;
}

public class Device
{
    public string Name;
    public string Ip;
    public string User;
    public bool isBuildHost;
    public string Comment;
}