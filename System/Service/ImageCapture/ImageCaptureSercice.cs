using Microsoft.AspNetCore.SignalR.Client;
using RoboticArmSystem.Core.Hubs.Modules;
using RoboticArmSystem.Core.Service.Config;
using RoboticArmSystem.Core.Service.ImageCapture.Models.Params;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoboticArmSystem.Core.Service.ImageCapture
{
    public class ImageCaptureSercice: IImageCaptureSercice
    {
        private SignalRClient _signalRClient;
        private string _eggIdentificationModelURL;

        public ImageCaptureSercice(ConfigService configService)
        {
            _signalRClient = new SignalRClient(configService.get("SignalRUrl"));
            _eggIdentificationModelURL = configService.get("EggIdentificationModelURL");
        }

        #region service
        public async Task ModelTrainingAsync(GetCameraCatchPhotoParam param)
        {

            TransmissionFormatParam formatParam = new TransmissionFormatParam
            {
                user_photo = param.user_photo,
                Base_axis = new List<int> { 115, 310 },
                photo_size = new List<int> { 640, 480 }
            };
            //1.Base_axis = new List<int> { 155, 327 },
            //2.Base_axis = new List<int> { 130, 313 },
            //3.Base_axis = new List<int> { 121, 311 },

            string jsonContent = JsonSerializer.Serialize(formatParam);

            using (HttpClient httpClient = new HttpClient())
            {
                //打內部 Server 的 AI 模型
                string pythonApiUrl = _eggIdentificationModelURL;

                HttpContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(pythonApiUrl, content);

                // 檢查發送請求是否成功
                if (response.IsSuccessStatusCode)
                {
                    // SignalR
                    _signalRClient.startSignalRConnect();

                    _signalRClient.Proxy.InvokeAsync("AIModal", "OK").Wait();

                    _signalRClient.stopSignalRConnect();

                    string responseContent = await response.Content.ReadAsStringAsync();


                    // ARM Raspberry
                    using ( HttpClient httpClientToPython = new HttpClient())
                    {
                        //打內部樹梅派 IP
                        string apiUrl = "http://172.20.10.5:5000/RespberryARM";

                        StringContent PostContent = new StringContent(responseContent, Encoding.UTF8, "application/json");

                        HttpResponseMessage PythonrResponse = await httpClientToPython.PostAsync(apiUrl, PostContent);

                        // Check if the response is successful
                        if (PythonrResponse.IsSuccessStatusCode)
                        {
                            // 資料集傳到樹梅派，如有資料資料錯誤請由樹梅派上架的FLASK去排查
                        }
                        else
                        {
                            throw new Exception("ERROR");
                        }

                    }

                }
                else
                {
                    // 處理請求失敗的情况
                    throw new Exception("接收無資料");
                }
            }

        }
        #endregion

    }
}
