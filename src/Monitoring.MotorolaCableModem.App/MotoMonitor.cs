using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace Monitoring.MotorolaCableModem.App
{
    static class MotoMonitor
    {
        public static async Task<string> RequestCMStatus(string baseUrl)
        {
            return await baseUrl
                .AppendPathSegment("HNAP1/")
                .WithHeader("SOAPACTION", "http://purenetworks.com/HNAP1/GetMultipleHNAPs")
                .PostStringAsync("{\"GetMultipleHNAPs\": {\"GetMotoStatusStartupSequence\": \"\", \"GetMotoStatusConnectionInfo\": \"\", \"GetMotoStatusDownstreamChannelInfo\": \"\", \"GetMotoStatusUpstreamChannelInfo\": \"\", \"GetMotoLagStatus\": \"\"}}")
                .ReceiveString();
        }

        public static async Task Login(string baseUrl, string username, string password)
        {
            await baseUrl
                .AppendPathSegment($"login_auth.html?loginUsername={username}&loginPassword={password}&")
                .GetAsync();
        }

        public static string GetTestPage()
        {
            using (var streamReader = new StreamReader("Response.json"))
                return streamReader.ReadToEnd();
        }

        public static string GetPassword()
        {
            using (var streamReader = new StreamReader("password.txt"))
                return streamReader.ReadToEnd().TrimEnd(' ', '\r', '\n');
        }

        public static (HnapResponse response,IEnumerable<ChannelStatus> downstream, IEnumerable<ChannelStatus> upstream, DateTimeOffset timestamp) ParseHnapResponse(string response)
        {
            var timestamp = DateTimeOffset.Now;

            var result = JsonConvert.DeserializeObject<HnapResponse>(response);
            var downstream = ParseDownstreamChannels(result.GetMultipleHNAPsResponse.GetMotoStatusDownstreamChannelInfoResponse.MotoConnDownstreamChannel);
            var upstream = ParseUpstreamChannels(result.GetMultipleHNAPsResponse.GetMotoStatusUpstreamChannelInfoResponse.MotoConnUpstreamChannel);

            return (result, downstream, upstream, timestamp);
        }

        static IEnumerable<ChannelStatus> ParseDownstreamChannels(string downstream)
        {
            var rows = downstream.Split("|+|");
            foreach (var row in rows)
            {
                var columns = row.Split("^");
                yield return new ChannelStatus
                {
                    Channel = int.Parse(columns[0]),
                    LockStatus = columns[1],
                    Modulation = columns[2],
                    ChannelId = int.Parse(columns[3]),
                    Frequency = decimal.Parse(columns[4]),
                    Power = float.Parse(columns[5]),
                    SNR = float.Parse(columns[6]),
                    CorrectedFrames = int.Parse(columns[7]),
                    UncorrectedFrames = int.Parse(columns[8])
                };
            }
        }

        static IEnumerable<ChannelStatus> ParseUpstreamChannels(string upstream)
        {
            var rows = upstream.Split("|+|");
            foreach (var row in rows)
            {
                var columns = row.Split("^");
                yield return new ChannelStatus
                {
                    Channel = int.Parse(columns[0]),
                    LockStatus = columns[1],
                    Modulation = columns[2],
                    ChannelId = int.Parse(columns[3]),
                    SymbolRate = int.Parse(columns[4]),
                    Frequency = decimal.Parse(columns[5]),
                    Power = float.Parse(columns[6]),
                };
            }
        }
    }
}
