using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Monitoring.Infrastructure;

namespace Monitoring.MotorolaCableModem.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var motoBase = "http://192.168.100.1";

            while (true)
            {
                try
                {
                    var cmStatus = await MotoMonitor.RequestCMStatus(motoBase);
                    if (cmStatus.Contains("parent.location='login.html'"))
                    {
                        Console.WriteLine("Login required");
                        var password = MotoMonitor.GetPassword();
                        await MotoMonitor.Login(motoBase, "admin", password);
                        cmStatus = await MotoMonitor.RequestCMStatus(motoBase);
                    }

                    //var cmStatus = MotoMonitor.GetTestPage();
                    var linkStatus = MotoMonitor.ParseLinkStatus(cmStatus);
                    var datapoints = linkStatus.Downstream.Select(cs => cs.ToInflux(isDownstream: true, measurement: "cm_link_status", linkStatus.Timestamp))
                                    .Concat(linkStatus.Upstream.Select(cs => cs.ToInflux(isDownstream: false, measurement: "cm_link_status", linkStatus.Timestamp)));
                    var datapointsAsString = InfluxUploader.DataPointsToString(datapoints);
                    await InfluxUploader.Upload("http://influxdb:8086", "drinka", datapointsAsString);
                }
                catch (Exception) { }

                Thread.Sleep(10000);
            }
        }
    }
}
