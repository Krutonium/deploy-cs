using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace deploy_cs;

public class targetCheck
{
    private bool checkIfHostOnline(string host)
    {
        try
        {
            // Check if host is online by connecting to port 22 and looking for "SSH"
            TcpClient client = new TcpClient();
            client.Connect(host, 22);
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            stream.ReadTimeout = 250; //250ms timeout
            string ssh = reader.ReadLine();
            client.Close();
            if (ssh.Contains("SSH"))
            {
                return true;
            }
            else
            {
                return false;
            }
        } catch (Exception e)
        {
            // The host is not online
            return false;
        }
    }
    public List<Device> GetOnlineDevices(List<Device> devices)
    {
        List<Device> onlineDevices = new List<Device>();
        Parallel.ForEach(devices, (device) =>
        {
            if (checkIfHostOnline(device.Ip))
            {
                onlineDevices.Add(device);
                Console.WriteLine("Device " + device.Name + " is online");
            }
            else
            {
                Console.WriteLine("Device " + device.Name + " is offline");
            }
        });
        return onlineDevices;
    }

    public List<Device> GetBuildHosts(List<Device> devices)
    {
        List<Device> buildHosts = new List<Device>();
        foreach(var device in devices)
        {
            if (device.isBuildHost)
            {
                buildHosts.Add(device);
                Console.WriteLine("Device " + device.Name + " is a build host");
            }
        }
        return buildHosts;
    }
}