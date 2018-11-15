using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Monitoring.Nest.App.Model
{
    public class ObjectHeader
    {
        public string ObjectKey { get; set; }
        public long ObjectTimestamp { get; set; }
        public int ObjectRevision { get; set; }
    }

    public class ObjectData : ObjectHeader
    {
        public JObject Value { get; set; }
    }

    public class ObjectDatas
    {
        public List<ObjectData> Objects { get; set; }
    }
}
