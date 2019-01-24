using System;
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
            await DeliverToDatabaseAsync(nestState);

            var lastTime = DateTimeOffset.Now;
            while (true)
            {
                var subscribeDelay = DateTimeOffset.Now - lastTime;
                if (subscribeDelay < TimeSpan.FromSeconds(10))
                {
                    Console.WriteLine($"{nameof(subscribeDelay)}={subscribeDelay} delaying={TimeSpan.FromSeconds(10) - subscribeDelay}");
                    await Task.Delay(TimeSpan.FromSeconds(10) - subscribeDelay);
                }
                else
                    Console.WriteLine($"{nameof(subscribeDelay)}={subscribeDelay}");
                lastTime += subscribeDelay;

                var newData = await nestClient.SubscribeAsync(nestState.Headers);
                nestState.UpdateData(newData);
                await DeliverToDatabaseAsync(nestState);
            }
        }

        static async Task DeliverToDatabaseAsync(NestState state)
        {
            var influxData = state.ToInfluxData();
            var influxLine = influxData.DataPointsToString();
            Console.WriteLine(influxLine);
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