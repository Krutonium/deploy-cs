namespace deploy_cs;

public class Targets
{
    public List<Device> Devices = new List<Device>();
}

public class Device
{
    // ReSharper disable here InconsistentNaming
    public string Name = "";
    public string Ip = "";
    public string User = "";
    public bool isDeployTarget = false;
    public string Comment = "";
}