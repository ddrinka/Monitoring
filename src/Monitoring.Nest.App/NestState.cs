using Monitoring.Nest.App.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monitoring.Nest.App
{
    public class NestState
    {
        readonly JsonSerializerSettings _serializerSettings;
        readonly Dictionary<string, ObjectData> _objectData = new Dictionary<string, ObjectData>();

        public NestState(JsonSerializerSettings serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public void UpdateData(IEnumerable<ObjectData> dataToUpdate)
        {
            foreach (var dataItem in dataToUpdate)
                _objectData[dataItem.ObjectKey] = dataItem;
        }

        public IEnumerable<string> Keys => _objectData.Keys;
        public IEnumerable<ObjectHeader> Headers => _objectData.Values;
        public DateTimeOffset LastUpdateTime { get; set; }
        public Dictionary<string, TimeSpan> CumulativeState { get; } = new Dictionary<string, TimeSpan>();
        public Dictionary<string, bool> PreviousState { get; } = new Dictionary<string, bool>();


        public bool TryGetObjectValue<T>(string key, out T value)
        {
            if (!_objectData.TryGetValue(key, out var objectData))
            {
                value = default;
                return false;
            }

            var valueStr = objectData.Value.ToString();
            value = JsonConvert.DeserializeObject<T>(valueStr, _serializerSettings);    //We can't use JObject.ToObject because it doesn't respect the Naming Strategy

            return true;
        }

        public T GetObjectValue<T>(string key)
        {
            if (!TryGetObjectValue<T>(key, out var result))
                throw new ArgumentException($"Key '{key}' not found");
            return result;
        }
    }
}
