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
                DeliverToDatabase();
                Thread.Sleep(60);
                var newData = await nestClient.SubscribeAsync(nestState.Headers);
                nestState.UpdateData(newData);
            }
        }

        static void DeliverToDatabase() { }
    }
}