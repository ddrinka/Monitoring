using Monitoring.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Monitoring.Nest.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var nestClient = new NestClient(username, password);
            var nestState = new NestState();
            var userData = await nestClient.GetUserDataAsync();
            nestState.UpdateData(userData.UpdatedBuckets);
            while (true)
            {
                await DeliverToDatabaseAsync(nestState);
                await Task.Delay(10000);
                var newData = await nestClient.SubscribeAsync(nestState.Headers);
                nestState.UpdateData(newData);
            }
        }

        static async Task DeliverToDatabaseAsync(NestState state)
        {
            var influxData = state.ToInfluxData();
            var influxLine = influxData.DataPointsToString();
            await InfluxUploader.Upload("http://influxdb:8086", "drinka", influxLine);
        }
    }
}