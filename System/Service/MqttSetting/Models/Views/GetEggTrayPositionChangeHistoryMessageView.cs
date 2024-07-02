using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.MqttSetting.Models.Views
{
    public class GetEggTrayPositionChangeHistoryMessageView
    {
        public string pId { get; set; }
        public string positionChange { get; set; }
        public string createtime { get; set; }
    }
}
