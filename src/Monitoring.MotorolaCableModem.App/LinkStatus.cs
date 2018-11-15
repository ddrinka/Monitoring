using System;
using System.Collections.Generic;

namespace Monitoring.MotorolaCableModem.App
{
    class LinkStatus
    {
        public DateTimeOffset Timestamp { get; set; }
        public IEnumerable<ChannelStatus> Upstream { get; set; }
        public IEnumerable<ChannelStatus> Downstream { get; set; }
    }
}
