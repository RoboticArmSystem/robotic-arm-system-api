using System;
using System.Collections.Generic;
using System.Text;

namespace RoboticArmSystem.Core.Service.ImageCapture.Models.Params
{
    public class TransmissionFormatParam
    {
        public string user_photo { get; set; }
        public List<int> Base_axis { get; set; }
        public List<int> photo_size { get; set; }
    }
}
