using RoboticArmSystem.Core.BaseService;
using RoboticArmSystem.Core.Service.ImageCapture.Models.Params;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmSystem.Core.Service.ImageCapture
{
    public interface IImageCaptureSercice: IService
    {
        Task ModelTrainingAsync(GetCameraCatchPhotoParam param);
    }
}
