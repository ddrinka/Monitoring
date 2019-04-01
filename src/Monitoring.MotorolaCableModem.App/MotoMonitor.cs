using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using HtmlAgilityPack;

namespace Monitoring.MotorolaCableModem.App
{
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
                .AppendPathSegment($"login_auth.html?loginUsername={username}&loginPassword={password}&")
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
            var rows = document.DocumentNode.SelectNodes("//td[contains(text(),'Downstream Bonded Channels')]/ancestor::table//tr[2]/td/table//tr");
            foreach (var row in rows.Skip(1))
            {
                var columns = row.SelectNodes("td");
                yield return new ChannelStatus
                {
                    Channel = int.Parse(columns[0].InnerText),
                    LockStatus = columns[1].InnerText,
                    Modulation = columns[2].InnerText,
                    ChannelId = int.Parse(columns[3].InnerText),
                    Frequency = decimal.Parse(columns[4].InnerText),
                    Power = float.Parse(columns[5].InnerText),
                    SNR = float.Parse(columns[6].InnerText),
                    CorrectedFrames = int.Parse(columns[7].InnerText)
                };
            }
        }

        static IEnumerable<ChannelStatus> ParseUpstreamChannels(HtmlDocument document)
        {
            var rows = document.DocumentNode.SelectNodes("//td[contains(text(),'Upstream Bonded Channels')]/ancestor::table//tr[2]/td/table//tr");
            foreach (var row in rows.Skip(1))
            {
                var columns = row.SelectNodes("td");
                yield return new ChannelStatus
                {
                    Channel = int.Parse(columns[0].InnerText),
                    LockStatus = columns[1].InnerText,
                    Modulation = columns[2].InnerText,
                    ChannelId = int.Parse(columns[3].InnerText),
                    SymbolRate = int.Parse(columns[4].InnerText),
                    Frequency = decimal.Parse(columns[5].InnerText),
                    Power = float.Parse(columns[6].InnerText),
                };
            }
        }
    }
}
