using System.IO;
using System.Threading.Tasks;
using Monitoring.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Monitoring.Nest.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            var (username, password) = GetUsernamePassword();
            var nestClient = new NestClient(username, password, serializerSettings);
            var nestState = new NestState(serializerSettings);
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

        static (string username, string password) GetUsernamePassword()
        {
            using (var streamReader = new StreamReader("password.txt"))
            {
                var line = streamReader.ReadLine().TrimEnd(' ');
                var split = line.Split(':');
                return (split[0], split[1]);
            }
        }
    }
}