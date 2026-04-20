using System;
using System.Diagnostics;

public partial class CpuCollector
{
    private class CpuCollectorWindows : ICpuImpl
    {
        private PerformanceCounter cpuCounter;   
        private PerformanceCounter[] cores;        
        public double totalLoad { get; private set; }
        public double[] coreLoad { get; private set; }

        public CpuCollectorWindows()
        {
            int coreCount = Environment.ProcessorCount;

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cores = new PerformanceCounter[coreCount];

            for (int i = 0; i < coreCount; i++)
            {
                cores[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            }

            cpuCounter.NextValue();

            foreach (var c in cores)
            {
                c.NextValue();
            }

            coreLoad = new double[coreCount];
        }

        public void Update()
        {
            totalLoad = cpuCounter.NextValue();

            for (int i = 0; i < cores.Length; i++)
            { 
                coreLoad[i] = cores[i].NextValue(); 
            }
        }

        public string GetFormattedValues()
        {
            string perCore = "";
            
            for (int i = 0; i < coreLoad.Length; i++)
            {
                perCore += $"{coreLoad[i]:F1}%";
                if (i != coreLoad.Length - 1) 
                { 
                    perCore += ", "; 
                }
            }

            return $"CPU: Total {totalLoad:F1}% | Per-core: [{perCore}]";
        }
    }
}