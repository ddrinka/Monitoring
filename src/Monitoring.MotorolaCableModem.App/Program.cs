using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using HtmlAgilityPack;

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
                    await InfluxUploader.Upload("http://influxdb:8088", "drinka", datapointsAsString);
                }
                catch (Exception) { }

                Thread.Sleep(10000);
            }
        }
    }

    static class MotoMonitor
    {
        public static async Task<string> RequestCMStatus(string baseUrl)
        {
            return await baseUrl
                .AppendPathSegment("MotoConnection.html")
                .GetStringAsync();
        }

        public static async Task Login(string baseUrl, string username, string password)
        {
            await baseUrl
                .AppendPathSegment("login_auth.html")
                .SetQueryParam("loginUsername", username)
                .SetQueryParam("loginPassword", password)
                .GetAsync();
        }

        public static string GetTestPage()
        {
            using (var streamReader = new StreamReader("Page.txt"))
                return streamReader.ReadToEnd();
        }

        public static string GetPassword()
        {
            using (var streamReader = new StreamReader("password.txt"))
                return streamReader.ReadToEnd().TrimEnd(' ', '\r', '\n');
        }

        public static LinkStatus ParseLinkStatus(string cmStatus)
        {
            var timestamp = DateTimeOffset.Now;

            var document = new HtmlDocument();
            document.LoadHtml(cmStatus);

            var result = new LinkStatus
            {
                Timestamp = timestamp,
                Downstream = ParseDownstreamChannels(document),
                Upstream = ParseUpstreamChannels(document)
            };

            return result;
        }

        static IEnumerable<ChannelStatus> ParseDownstreamChannels(HtmlDocument document)
        {
            var rows = document.DocumentNode.SelectNodes("//td[contains(text(),'Downstream Bonded Channels')]/ancestor::table/tr[2]/td/table/tr");
            foreach (var row in rows.Skip(1))
            {
                var columns = row.SelectNodes("td");
                yield return new ChannelStatus
                {
                    Channel=int.Parse(columns[0].InnerText),
                    LockStatus=columns[1].InnerText,
                    Modulation=columns[2].InnerText,
                    ChannelId=int.Parse(columns[3].InnerText),
                    Frequency=decimal.Parse(columns[4].InnerText),
                    Power=float.Parse(columns[5].InnerText),
                    SNR=float.Parse(columns[6].InnerText),
                    CorrectedFrames=int.Parse(columns[7].InnerText)
                };
            }
        }

        static IEnumerable<ChannelStatus> ParseUpstreamChannels(HtmlDocument document)
        {
            var rows = document.DocumentNode.SelectNodes("//td[contains(text(),'Upstream Bonded Channels')]/ancestor::table/tr[2]/td/table/tr");
            foreach (var row in rows.Skip(1))
            {
                var columns = row.SelectNodes("td");
                yield return new ChannelStatus
                {
                    Channel = int.Parse(columns[0].InnerText),
                    LockStatus = columns[1].InnerText,
                    Modulation = columns[2].InnerText,
                    ChannelId = int.Parse(columns[3].InnerText),
                    SymbolRate=int.Parse(columns[4].InnerText),
                    Frequency = decimal.Parse(columns[5].InnerText),
                    Power = float.Parse(columns[6].InnerText),
                };
            }
        }
    }

    static class InfluxExtensions
    {
        public static InfluxData ToInflux(this ChannelStatus c, bool isDownstream, string measurement, DateTimeOffset timestamp)
        {
            return new InfluxData
            {
                Measurement=measurement,
                Timestamp=timestamp,
                Tags = new Dictionary<string, string>
                {
                    { "channel_id", c.ChannelId.ToString() },
                    { "direction", isDownstream?"downstream":"upstream" },
                    { "frequency", c.Frequency.ToString() }
                },
                Fields = new Dictionary<string, string>
                {
                    { "lock_status", Quote(c.LockStatus) },
                    { "modulation", Quote(c.Modulation) },
                    { "symbol_rate", c.SymbolRate.HasValue?$"{c.SymbolRate}i":null },
                    { "power", $"{c.Power}" },
                    { "snr", c.SNR.HasValue?$"{c.SNR}":null },
                    { "corrected_frames", c.CorrectedFrames.HasValue?$"{c.CorrectedFrames}i":null },
                    { "uncorrected_frames", c.UncorrectedFrames.HasValue?$"{c.UncorrectedFrames}i":null }
                }
            };
        }

        public static string ToLineProtocol(this InfluxData d)
        {
            var sb = new StringBuilder();
            sb.Append(d.Measurement);
            foreach(var tag in d.Tags.OrderBy(x => x.Key))
            {
                sb.Append($",{tag.Key}={tag.Value}");
            }
            sb.Append(" ");
            bool firstField = true;
            foreach(var field in d.Fields)
            {
                if (field.Value == null)
                    continue;

                if (!firstField)
                    sb.Append(",");
                else
                    firstField = false;

                sb.Append($"{field.Key}={field.Value}");
            }

            sb.Append($" {d.Timestamp.Ticks}00");     //Ticks=nanoseconds/100. Don't multiply here to avoid overflowing long

            return sb.ToString();
        }

        static string Quote(string inner)
        {
            return $"\"{inner}\"";
        }
    }

    static class InfluxUploader
    {
        public static string DataPointsToString(IEnumerable<InfluxData> datapoints)
        {
            var sb = new StringBuilder();
            foreach (var datapoint in datapoints)
            {
                sb.AppendLine(datapoint.ToLineProtocol());
            }

            return sb.ToString();
        }

        public static async Task Upload(string baseUrl, string database, string data)
        {
            await baseUrl
                .AppendPathSegment("write")
                .SetQueryParam("database", database)
                .PostStringAsync(data)
            ;
        }
    }

    class ChannelStatus
    {
        public int Channel { get; set; }
        public string LockStatus { get; set; }
        public string Modulation { get; set; }
        public int ChannelId { get; set; }
        public int? SymbolRate { get; set; }
        public decimal Frequency { get; set; }
        public float Power { get; set; }
        public float? SNR { get; set; }
        public int? CorrectedFrames { get; set; }
        public int? UncorrectedFrames { get; set; }
    }

    class LinkStatus
    {
        public DateTimeOffset Timestamp { get; set; }
        public IEnumerable<ChannelStatus> Upstream { get; set; }
        public IEnumerable<ChannelStatus> Downstream { get; set; }
    }

    class InfluxData
    {
        public string Measurement { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public IDictionary<string,string> Tags { get; set; }
        public IDictionary<string,string> Fields { get; set; }
    }
}
