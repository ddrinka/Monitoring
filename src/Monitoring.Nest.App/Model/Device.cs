using Monitoring.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monitoring.Nest.App.Model
{
    public class Device
    {
        public float BatteryLevel { get; set; }
        public float CurrentHumidity { get; set; }
        public string CurrentScheduleMode { get; set; }
        public string ErrorCode { get; set; }
        public bool FanControlState { get; set; }
        public bool FanCoolingState { get; set; }
        public string FanCurrentSpeed { get; set; }
        public string FanMode { get; set; }
        public bool IsFurnaceShutdown { get; set; }
        public bool PreconditioningActive { get; set; }
        public float RSSI { get; set; }
        public string SafetyState { get; set; }
        public bool SunlightCorrectionActive { get; set; }
        public float TargetHumidity { get; set; }
        public float TimeToTarget { get; set; }

        [ExcludeFromInflux]
        public string WhereId { get; set; }
    }
}
