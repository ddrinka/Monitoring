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
                var response = await MotoMonitor.RequestCMStatus(motoBase);
                if (response.Contains("parent.location='login.html'"))
                {
                    Console.WriteLine("Login required");
                    var password = MotoMonitor.GetPassword();
                    await MotoMonitor.Login(motoBase, "admin", password);
                    response = await MotoMonitor.RequestCMStatus(motoBase);
                }

                //var response = MotoMonitor.GetTestPage();
                var linkStatus = MotoMonitor.ParseHnapResponse(response);
                var datapoints = linkStatus.downstream.Select(cs => cs.ToInflux(isDownstream: true, measurement: "cm_link_status", linkStatus.timestamp))
                                .Concat(linkStatus.upstream.Select(cs => cs.ToInflux(isDownstream: false, measurement: "cm_link_status", linkStatus.timestamp)));
                var datapointsAsString = InfluxUploader.DataPointsToString(datapoints);
                await InfluxUploader.Upload("http://influxdb:8086", "drinka", datapointsAsString);

                Thread.Sleep(10000);
            }
        }
    }
}
