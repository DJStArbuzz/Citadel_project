using System;

public partial class CpuCollector : MetricCollector
{
    private readonly ICpuImpl impl;

    public CpuCollector()
    {
#if WINDOWS
        impl = new CpuCollectorWindows();
#elif LINUX
        impl = new CpuCollectorLinux();
#else
        throw new PlatformNotSupportedException("ne Windows and ne Linux, a znachit ne smotrim");
#endif
    }

    public override void Update()
    {
        impl.Update();
    }

    public override string GetFormattedValues()
    {
        return impl.GetFormattedValues();
    }

    private interface ICpuImpl
    {
        void Update();
        string GetFormattedValues();
    }
}