using System;
using System.Collections.Generic;
using System.Text;

namespace Monitoring.Infrastructure
{
    public static class DateTimeOffsetHelper
    {
        readonly static DateTimeOffset _unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static DateTimeOffset FromUnixEpochMilliseconds(int milliseconds)
        {
            return _unixEpoch.AddTicks(milliseconds * TimeSpan.TicksPerMillisecond);
        }
    }
}
