namespace RoboticArmSystem.Core.Response
{
    public class ResponseModel
    {
        public class ApiResponse<T>
        {
            public bool isSuccess { get; set; }

            public string message { get; set; }

            public T data { get; set; }

            public ApiResponse(T data, bool isSuccess, string message = "")
            {
                this.data = data;
                this.isSuccess = isSuccess;
                this.message = message;
            }
        }
    }
}
