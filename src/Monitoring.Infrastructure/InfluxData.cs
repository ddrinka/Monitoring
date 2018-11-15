using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitoring.Infrastructure
{
    public class InfluxData
    {
        public string Measurement { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public IDictionary<string, string> Tags { get; set; }
        public IDictionary<string, string> Fields { get; set; }
    }

    public static class InfluxDataExtensions
    {
        readonly static DateTimeOffset _unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        public static string ToLineProtocol(this InfluxData d)
        {
            var sb = new StringBuilder();
            sb.Append(d.Measurement);
            foreach (var tag in d.Tags.OrderBy(x => x.Key))
            {
                sb.Append($",{tag.Key}={tag.Value}");
            }
            sb.Append(" ");
            bool firstField = true;
            foreach (var field in d.Fields)
            {
                if (field.Value == null)
                    continue;

                if (!firstField)
                    sb.Append(",");
                else
                    firstField = false;

                sb.Append($"{field.Key}={field.Value}");
            }

            var unixTimeTicks = (d.Timestamp - _unixEpoch).Ticks;
            sb.Append($" {unixTimeTicks}00");     //Ticks=nanoseconds/100. Don't multiply here to avoid overflowing long

            return sb.ToString();
        }
    }
}
