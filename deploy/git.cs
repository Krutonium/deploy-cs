using System.Diagnostics;

namespace deploy;

public class git
{
    private static ProcessStartInfo info;
    private static void gitPull()
    {
        Console.WriteLine("Doing Git Pull");
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
        Console.WriteLine("Doing Git Push");
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
        Console.WriteLine("Doing Git Add");
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
        Console.WriteLine("Doing Git Commit");
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
        Console.Title = "Git Sync";
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