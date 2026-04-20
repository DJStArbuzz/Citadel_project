using System;
using System.Linq;
using System.Net.NetworkInformation;

public class NetworkCollector : MetricCollector
{
    private long prevRx;
    private long prevTx;

    private DateTime prevTime;

    public double rxSpeed { get; private set; }
    public double txSpeed { get; private set; }

    public NetworkCollector()
    {
        prevTime = DateTime.UtcNow;
        Update(); 
    }

    public override void Update()
    {
        var now = DateTime.UtcNow;
        double seconds = Math.Max((now - prevTime).TotalSeconds, 0.001);

        long rx = 0;
        long tx = 0; 

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                var stats = ni.GetIPv4Statistics();
                rx += stats.BytesReceived;
                tx += stats.BytesSent;
            }
        }

        if (prevRx > 0 && prevTx > 0)
        {
            rxSpeed = Math.Max((rx - prevRx) / seconds, 0);
            txSpeed = Math.Max((tx - prevTx) / seconds, 0);
        }

        prevRx = rx;
        prevTx = tx;
        prevTime = now;
    }

    public override string GetFormattedValues()
    {
        return $"Network: ↓ {rxSpeed:F0} B/s, ↑ {txSpeed:F0} B/s";
    }
}