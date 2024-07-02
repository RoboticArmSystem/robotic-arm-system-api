using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.MqttSetting.Models.Params
{
    public class UpdateMQTTConnectionInfoParam
    {
        public string ip { get; set; }
        public string port { get; set; }
        public string topic { get; set; }
    }
}
