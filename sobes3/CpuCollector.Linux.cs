using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public partial class CpuCollector
{
    private class CpuCollectorLinux : ICpuImpl
    {
        private LinuxCpuData data;         
        public double totalUsage { get; private set; }
        public double[] perCoreUsage { get; private set; }

        public CpuCollectorLinux()
        {
            perCoreUsage = new double[Environment.ProcessorCount];
            data = new LinuxCpuData();
            data.Update();
        }

        public void Update()
        {
            data.Update();
            totalUsage = data.totalUsageL;
            perCoreUsage = data.perCoreUsageL;
        }

        public string GetFormattedValues()
        {
            string perCore = string.Join(", ", perCoreUsage.Select(p => $"{p:F1}%"));
            return $"CPU: Total {totalUsage:F1}% | Per-core: [{perCore}]";
        }

        private class LinuxCpuData
        {
            private Dictionary<int, CpuStats> prevStats = new();   
            public double totalUsageL { get; private set; }
            public double[] perCoreUsageL { get; private set; }

            public LinuxCpuData()
            {
                perCoreUsageL = new double[Environment.ProcessorCount];
            }

            public void Update()
            {
                var lines = File.ReadAllLines("/proc/stat");
                var totalLine = lines.First(l => l.StartsWith("cpu "));
                var coreLines = lines.Where(l => Regex.IsMatch(l, @"^cpu\d+")).Take(Environment.ProcessorCount).ToList();

                var currentTotal = ParseLine(totalLine);
                totalUsageL = CalculateUsage(currentTotal, prevStats.GetValueOrDefault(-1));

                for (int i = 0; i < coreLines.Count; i++)
                {
                    var current = ParseLine(coreLines[i]);
                    var prev = prevStats.GetValueOrDefault(i);
                    perCoreUsageL[i] = CalculateUsage(current, prev);
                    prevStats[i] = current;
                }
                prevStats[-1] = currentTotal;
            }

            private CpuStats ParseLine(string line)
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 8) return new CpuStats();
                return new CpuStats
                {
                    User = long.Parse(parts[1]),
                    Nice = long.Parse(parts[2]),
                    System = long.Parse(parts[3]),
                    Idle = long.Parse(parts[4]),
                    Iowait = parts.Length > 5 ? long.Parse(parts[5]) : 0,
                    Irq = parts.Length > 6 ? long.Parse(parts[6]) : 0,
                    Softirq = parts.Length > 7 ? long.Parse(parts[7]) : 0,
                    Steal = parts.Length > 8 ? long.Parse(parts[8]) : 0
                };
            }

            private long GetIdle(CpuStats s) => s.Idle + s.Iowait;
            private long GetActive(CpuStats s) => s.User + s.Nice + s.System + s.Irq + s.Softirq + s.Steal;

            private double CalculateUsage(CpuStats curr, CpuStats prev)
            {
                long prevIdle = GetIdle(prev);
                long prevActive = GetActive(prev);
                long currIdle = GetIdle(curr);
                long currActive = GetActive(curr);
                long totalDelta = (currActive + currIdle) - (prevActive + prevIdle);
                if (totalDelta == 0) return 0;
                long activeDelta = currActive - prevActive;
                return (double)activeDelta / totalDelta * 100.0;
            }

            private struct CpuStats
            {
                public long User, Nice, System, Idle, Iowait, Irq, Softirq, Steal;
            }
        }
    }
}