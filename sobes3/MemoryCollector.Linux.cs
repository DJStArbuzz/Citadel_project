using System;
using System.IO;
using System.Linq;

public partial class MemoryCollector
{
    private class MemoryCollectorLinux : IMemoryImpl
    {
        public double totalMemoryMB { get; private set; }
        public double availableMemoryMB { get; private set; }
        public double usagePercent { get; private set; }

        public void Update()
        {
            string memInfo = File.ReadAllText("/proc/meminfo");
            var lines = memInfo.Split('\n');

            long totalKB = 0;
            long availableKB = 0;
            
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:"))
                { 
                    totalKB = ParseMeminfoValue(line); 
                }

                else if (line.StartsWith("MemAvailable:"))
                {
                    availableKB = ParseMeminfoValue(line);
                }
            }
            totalMemoryMB = totalKB / 1024.0;
            availableMemoryMB = availableKB / 1024.0;
            usagePercent = (totalMemoryMB - availableMemoryMB) / totalMemoryMB * 100.0;
        }

        private long ParseMeminfoValue(string line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && long.TryParse(parts[1], out long value))
            {
                return value;
            }
            return 0;
        }

        public string GetFormattedValues()
        {
            return $"Memory: Total {totalMemoryMB:F0} MB, Available {availableMemoryMB:F0} MB ({usagePercent:F1}%)";
        }
    }
}