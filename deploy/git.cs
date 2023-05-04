using System.Diagnostics;

namespace deploy;

public class git
{
    private static ProcessStartInfo info;
    private static void gitPull()
    {
        Process _process = new Process();
        _process.StartInfo = info;
        info.Arguments = "pull";
        _process.Start();
        _process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.WaitForExit();
    }

    private static void gitPush()
    {
        info.Arguments = "push";
        Process _process = new Process();
        _process.StartInfo = info;
        info.Arguments = "push";
        _process.Start();
        _process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.WaitForExit();
    }
    private static void gitAdd()
    {
        Process _process = new Process();
        _process.StartInfo = info;
        info.Arguments = "add .";
        _process.Start();
        _process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.WaitForExit();
    }
    private static void gitCommit()
    {
        Process _process = new Process();
        _process.StartInfo = info;
        info.Arguments = "commit -m \"Deploy\"";
        _process.Start();
        _process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        _process.WaitForExit();
    }

    public static void gitSync(string path)
    {
        Console.WriteLine("Doing Git Sync");
        info = new ProcessStartInfo();
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