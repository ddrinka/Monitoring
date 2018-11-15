using System;
using System.Collections.Generic;
using System.Text;

namespace Monitoring.Nest.App.Model
{
    public class Shared
    {
        public int AutoAway { get; set; }
        public bool CanCool { get; set; }
        public bool CanHeat { get; set; }
        public bool CompressorLockoutEnabled { get; set; }
        public float CurrentTemperature { get; set; }
        public bool HvacAcState { get; set; }
        public bool HvacFanState { get; set; }
        public bool HvacHeaterState { get; set; }
        public bool TargetChangePending { get; set; }
        public float TargetTemperature { get; set; }
        public float TargetTemperatureHigh { get; set; }
        public float TargetTemperatureLow { get; set; }
        public string TargetTemperatureType { get; set; }
    }
}
