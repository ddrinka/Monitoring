using Monitoring.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monitoring.Nest.App.Model
{
    public class Kryptonite
    {
        public float BatteryLevel { get; set; }
        public float CurrentTemperature { get; set; }

        [ExcludeFromInflux]
        public string WhereId { get; set; }
    }
}
