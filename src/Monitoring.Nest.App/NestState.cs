using Monitoring.Nest.App.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monitoring.Nest.App
{
    public class NestState
    {
        readonly Dictionary<string, ObjectData> _objectData = new Dictionary<string, ObjectData>();

        public void UpdateData(IEnumerable<ObjectData> dataToUpdate)
        {
            foreach (var dataItem in dataToUpdate)
                _objectData[dataItem.ObjectKey] = dataItem;
        }

        public IEnumerable<string> Keys => _objectData.Keys;
        public IEnumerable<ObjectHeader> Headers => _objectData.Values;

        public bool TryGetObjectValue<T>(string key, out T value)
        {
            if (!_objectData.TryGetValue(key, out var objectData))
            {
                value = default;
                return false;
            }

            value = objectData.Value.ToObject<T>();
            return true;
        }
    }
}
