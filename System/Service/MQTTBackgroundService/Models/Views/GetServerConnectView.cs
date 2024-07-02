using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.MQTTBackgroundService.Models.Views
{
    public class GetServerConnectView
    {
        public string ip { get; set; }
        public string port { get; set; }
        public string topic { get; set; }
    }
}
