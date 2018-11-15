using Monitoring.Infrastructure;
using Monitoring.Nest.App.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitoring.Nest.App
{
    public static class InfluxExtensions
    {
        public static IEnumerable<InfluxData> ToInfluxData(this NestState state)
        {
            var timestamp = DateTimeOffset.Now;

            var whereKey = state.Keys.Where(key => key.StartsWith("where.")).Single();
            var whereList = state.GetObjectValue<WhereList>(whereKey);

            foreach(var deviceKey in state.Keys.Where(key => key.StartsWith("device.")))
            {
                var deviceId = deviceKey.Split('.')[1];
                var sharedKey = $"shared.{deviceId}";
                var deviceDetails = state.GetObjectValue<Device>(deviceKey);
                var sharedDetails = state.GetObjectValue<Shared>(sharedKey);
                yield return ToInfluxData(deviceDetails, sharedDetails, whereList.Wheres, timestamp);
            }

            foreach(var kryptoniteKey in state.Keys.Where(key=>key.StartsWith("kryptonite.")))
            {
                var kryptoniteDetails = state.GetObjectValue<Kryptonite>(kryptoniteKey);
                yield return ToInfluxData(kryptoniteDetails, whereList.Wheres, timestamp);
            }
        }

        public static InfluxData ToInfluxData(Device deviceDetails, Shared sharedDetails, List<Where> whereList, DateTimeOffset timestamp)
        {
            string where = whereList.Where(w => w.WhereId == deviceDetails.WhereId).Single().Name;

            var fields = InfluxKeyValueHelper.GetInfluxKeyValuesFromPoco(deviceDetails);
            foreach (var keyVal in InfluxKeyValueHelper.GetInfluxKeyValuesFromPoco(sharedDetails))
                fields[keyVal.Key] = keyVal.Value;


            return new InfluxData
            {
                Measurement = "nest",
                Timestamp = timestamp,
                Tags = new Dictionary<string, string> { { "where", where } },
                Fields = fields
            };
        }

        public static InfluxData ToInfluxData(Kryptonite kryptoniteDetails, List<Where> whereList, DateTimeOffset timestamp)
        {
            string where = whereList.Where(w => w.WhereId == kryptoniteDetails.WhereId).Single().Name;

            var fields = InfluxKeyValueHelper.GetInfluxKeyValuesFromPoco(kryptoniteDetails);

            return new InfluxData
            {
                Measurement = "nest_remote",
                Timestamp = timestamp,
                Tags = new Dictionary<string, string> { { "where", where } },
                Fields = fields
            };
        }
    }
}
