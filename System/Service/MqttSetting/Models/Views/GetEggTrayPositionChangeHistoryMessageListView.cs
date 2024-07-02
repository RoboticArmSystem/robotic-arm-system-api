using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.MqttSetting.Models.Views
{
    public class GetEggTrayPositionChangeHistoryMessageListView
    {
        public List<GetEggTrayPositionChangeHistoryMessageView> positionChangeHistoryMessageViews { get; set; }
    }
}
