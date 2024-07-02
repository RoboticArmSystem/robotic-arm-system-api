using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.MqttSetting.Models.Views
{
    public class SubscribeReceiveMessageView
    {
        private List<string> _receiveMessag;
        public List<string> receiveMessag
        {
            get { return _receiveMessag; }
            set { _receiveMessag = value; }
        }
    }
}
