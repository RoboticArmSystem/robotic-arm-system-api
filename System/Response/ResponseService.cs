using static RoboticArmSystem.Core.Response.ResponseModel;

namespace RoboticArmSystem.Core.Response
{
    public class ResponseService
    {
        public ApiResponse<T> normal<T>(T data, bool isSuccess, string message = "")
        {
            return new ApiResponse<T>(data, isSuccess, message);
        }
    }
}
