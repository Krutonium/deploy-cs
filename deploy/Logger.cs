using System.Text;

namespace deploy;

public class Logger
{
    private StringBuilder logger = new StringBuilder();
    public string path = "/tmp/deploy.log";
    public void Log(string message)
    {
        logger.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message);
        File.AppendAllText(path, logger.ToString());
        logger.Clear();
    }
}