using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.MQTTBackgroundService.Models.Views
{
    public  class GetLastEggTrayPositionDataView
    {
        public string pId { get; set; }
        public string p1 { get; set; }
        public string p2 { get; set; }
        public string p3 { get; set; }
        public string p4 { get; set; }
        public string p5 { get; set; }
    }
}
