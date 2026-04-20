using System;

public partial class MemoryCollector : MetricCollector
{
    private readonly IMemoryImpl impl;

    public MemoryCollector()
    {
#if WINDOWS
        impl = new MemoryCollectorWindows();
#elif LINUX
        impl = new MemoryCollectorLinux();
#else
        throw new PlatformNotSupportedException("Only Windows and Linux are supported.");
#endif
    }

    public override void Update() { 
        impl.Update(); 
    }

    public override string GetFormattedValues()
    {
        return impl.GetFormattedValues();
    }

    private interface IMemoryImpl
    {
        void Update();
        string GetFormattedValues();
    }
}