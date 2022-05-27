namespace deploy_cs;

public class Targets
{
    public List<Device> Devices = new List<Device>();

    public bool BuildHostEnabled = false;
    public bool ParallelDeploy = true;
}

public class Device
{
    // ReSharper disable here InconsistentNaming
    public string Name = "";
    public string Ip = "";
    public string User = "";
    public bool isBuildHost = false;
    public bool isDeployTarget = false;
    public string Comment = "It forced me to do this to remove warnings.";
}