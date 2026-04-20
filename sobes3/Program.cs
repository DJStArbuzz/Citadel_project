using System;
using System.IO;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("\nStop");
            Environment.Exit(0);
        };

        var config = Configuration.Load();
        var cpu = new CpuCollector();
        var mem = new MemoryCollector();
        var net = new NetworkCollector();

        var logDir = Path.GetDirectoryName(config.logFilePath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        bool logToFile = config.resultFormat.Equals("File", StringComparison.OrdinalIgnoreCase);
        StreamWriter? fileWriter = null;
        
        if (logToFile)
        {
            fileWriter = new StreamWriter(config.logFilePath, append: true);
            fileWriter.WriteLine($"--- Monitor started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
            fileWriter.Flush();
        }

        while (true)
        {
            cpu.Update();
            mem.Update();
            net.Update();

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string output = $"[{timestamp}] {cpu.GetFormattedValues()} | {mem.GetFormattedValues()} | {net.GetFormattedValues()}";

            if (logToFile && fileWriter != null)
            {
                fileWriter.WriteLine(output);
                fileWriter.Flush();
            }
            else
            {
                Console.WriteLine(output);
            }

            Thread.Sleep(config.period * 1000);
        }
    }
}