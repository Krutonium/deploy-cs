using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace deploy_cs;

public class targetCheck
{
    internal bool checkIfHostOnline(string host)
    {
        // Check if host is online by connecting to port 22 and looking for "SSH"
        TcpClient client = new TcpClient();
        client.Connect(host, 22);
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
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
    }
}