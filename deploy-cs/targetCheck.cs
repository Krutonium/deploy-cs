using System.Net.NetworkInformation;

namespace deploy_cs;

public class targetCheck
{
    internal bool checkIfHostOnline(string host)
    {
        var ping = new Ping();
        var result = ping.Send(host);
        if (result.Status != System.Net.NetworkInformation.IPStatus.Success)
        {
            return false;
        }
        return true;
    }
}