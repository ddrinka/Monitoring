using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace Monitoring.Infrastructure
{
    public static class InfluxUploader
    {
        public static string DataPointsToString(this IEnumerable<InfluxData> datapoints)
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
                .SetQueryParam("db", database)
                .PostStringAsync(data)
            ;
        }
    }

}
