using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.MqttSetting.Models.Params
{
    public class MqttConnectionLogMessageParam
    {
        public DateTime createtime { get; set; }
        public string ip { get; set; }
        public string topic { get; set; }
        public string message { get; set; }
    }
}
