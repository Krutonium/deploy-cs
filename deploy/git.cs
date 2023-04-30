using System.Diagnostics;

namespace deploy;

public class git
{
    private static Process _process;
    private static ProcessStartInfo info;
    private static void gitPull()
    {
        info.Arguments = "pull";
        _process.Start();
        _process.WaitForExit();
    }

    private static void gitPush()
    {
        info.Arguments = "push";
        _process.Start();
        _process.WaitForExit();
    }
    private static void gitAdd()
    {
        info.Arguments = "add .";
        _process.Start();
        _process.WaitForExit();
    }
    private static void gitCommit()
    {
        info.Arguments = "commit";
        _process.Start();
        _process.WaitForExit();
    }

    public static void gitSync(string path)
    {
        Console.WriteLine("Doing Git Sync");
        _process = new Process();
        info = new ProcessStartInfo();
        _process.StartInfo = info;
        _process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        info.RedirectStandardError = true;
        info.RedirectStandardOutput = true;
        info.FileName = "git";
        info.WorkingDirectory = path;
        gitAdd();
        gitCommit();
        gitPull();
        gitPush();
    }
}