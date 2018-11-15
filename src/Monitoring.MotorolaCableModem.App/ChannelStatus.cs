using Monitoring.Infrastructure;
using System;
using System.Collections.Generic;

namespace Monitoring.MotorolaCableModem.App
{
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

    static class ChannelStatusExtensions
    {
        public static InfluxData ToInflux(this ChannelStatus c, bool isDownstream, string measurement, DateTimeOffset timestamp)
        {
            return new InfluxData
            {
                Measurement = measurement,
                Timestamp = timestamp,
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

        static string Quote(string inner)
        {
            return $"\"{inner}\"";
        }
    }
}
