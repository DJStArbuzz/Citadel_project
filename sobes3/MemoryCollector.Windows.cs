using System;
using System.Runtime.InteropServices;

public partial class MemoryCollector
{
    private class MemoryCollectorWindows : IMemoryImpl
    {
        [DllImport("kernel32.dll")]
        static extern void GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        struct MEMORYSTATUSEX
        {
            public uint memSize;           
            public uint memLoad;          
            public ulong memTotalPhys;   
            public ulong memAvailPhys;   
            public ulong memTotalPage;     
            public ulong memAvailPage;   
            public ulong memTotalVirtual;  
            public ulong memAvailVirtual;
            public ulong memAvailExtended; 
        }

        public double totalMemoryMB { get; private set; }
        public double availableMemoryMB { get; private set; }
        public double usagePercent { get; private set; }

        public void Update()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            memStatus.memSize = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            GlobalMemoryStatusEx(ref memStatus);

            totalMemoryMB = (double)memStatus.memTotalPhys / (1024 * 1024);
            availableMemoryMB = (double)memStatus.memAvailPhys / (1024 * 1024);
            usagePercent = (totalMemoryMB - availableMemoryMB) / totalMemoryMB * 100.0;
        }

        public string GetFormattedValues()
        {
            return $"Memory: Total {totalMemoryMB:F0} MB, Available {availableMemoryMB:F0} MB ({usagePercent:F1}%)";
        }
    }
}