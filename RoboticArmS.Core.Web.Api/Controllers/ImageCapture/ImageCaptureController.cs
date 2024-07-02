using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoboticArmSystem.Core.Response;
using RoboticArmSystem.Core.Service.ImageCapture;
using RoboticArmSystem.Core.Service.ImageCapture.Models.Params;
using static RoboticArmSystem.Core.Response.ResponseModel;

namespace RoboticArmSystem.Core.Web.Api.Controllers.ImageCapture
{
    [Route("RoboticArm/[controller]")]
    [ApiController]
    public class ImageCaptureController : ControllerBase
    {
        private readonly ResponseService _responseService;
        private readonly IImageCaptureSercice _imageCaptureSercice;

        public ImageCaptureController(ResponseService responseService, ImageCaptureSercice imageCaptureSercice)
        {
            _responseService = responseService;
            _imageCaptureSercice = imageCaptureSercice;
        }

        [HttpPost("PoshImage")]
        public async Task<ApiResponse<object>> GetCameraCatchPhoto([FromBody] GetCameraCatchPhotoParam param)
        {
            try
            {
                await _imageCaptureSercice.ModelTrainingAsync(param);
                return _responseService.normal<object>("", true, "");
            }
            catch
            {
                return _responseService.normal<object>("", false, "");
            }
        }
    }
}
