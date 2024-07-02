using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoboticArmSystem.Core.Response;
using RoboticArmSystem.Core.Service.MQTTBackgroundService;
using RoboticArmSystem.Core.Service.MQTTBackgroundService.Models.Views;
using RoboticArmSystem.Core.Service.MqttSetting;
using RoboticArmSystem.Core.Service.MqttSetting.Models.Params;
using RoboticArmSystem.Core.Service.MqttSetting.Models.Views;
using static RoboticArmSystem.Core.Response.ResponseModel;

namespace RoboticArmSystem.Core.Web.Api.Controllers.mqttSetting
{
    [Route("RoboticArm/[controller]")]
    [ApiController]
    public class MqttSettingController : ControllerBase
    {
        private readonly IMQTTService _MQTTService;
        private readonly ResponseService _responseService;
        private readonly MQTTBackgroundService _MQTTBackgroundService;

        // 此 MQTT API 是狀態設定 
        public MqttSettingController(ResponseService responseService, MQTTService MQTTService, MQTTBackgroundService MQTTBackgroundService)
        {
            _responseService = responseService;
            _MQTTService = MQTTService;
            _MQTTBackgroundService = MQTTBackgroundService;
        }

        /// <summary>
        /// 獲取MQTT連線狀態 / 瑋程 / 20231026
        /// </summary>
        [HttpGet("GetMqttConnectionStatus")]
        public ApiResponse<object> GetSetMqttConnectionStatus()
        {
            try
            {
                return _responseService.normal<object>(_MQTTBackgroundService.SetMqttConnectionStatus, true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal<object>("", false, ex.Message);
            }
        }

        /// <summary>
        /// 獲取MQTT連線資料 / 瑋程 / 20231027
        /// </summary>
        [HttpGet("GetMqttConnectionInfo")]
        public async Task<ApiResponse<GetMqttConnectionInfoView>> GetMqttConnectionInfoAsync()
        {
            try
            {
                var data = await _MQTTService.GetMqttConnectionInfoAsync();
                return _responseService.normal(data, true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal(new GetMqttConnectionInfoView { }, false, ex.Message);
            }
        }

        /// <summary>
        /// 更新背景MQTT服務連線 / 瑋程 / 20231027
        /// </summary>
        [HttpPost("UpdateMQTTBackgroundServiceConnectionInfo")]
        public async Task<ApiResponse<object>> UpdateMqttBackgroundServiceConnectionInfoAsync([FromBody] UpdateMQTTConnectionInfoParam param)
        {
            try
            {
                //更新背景執行的MQTT連線IP與TOPIC
                await _MQTTBackgroundService.UpdateMqttConnectionInfoAsync(param.ip, param.topic);
                //修改資料庫的MQTT連線資料
                await _MQTTService.UpdateMqttConnectionInfoAsync(param);
                //新增資料庫的LOG成功訊息
                await _MQTTService.InsertMqttConnectionLogAsync(param, "連線成功");

                return _responseService.normal<object>(_MQTTBackgroundService.SetMqttConnectionStatus, true, "IP：" + param.ip + "連線成功");
            }
            catch (Exception ex)
            {

                if (ex.Message.Contains("Error while connecting with host"))
                {
                    //新增錯誤LOG
                    await _MQTTService.InsertMqttConnectionLogAsync(param, "無法成功連接MQTT伺服器");
                    return _responseService.normal<object>("", false, "IP：" + param.ip + "無法成功連接MQTT伺服器");
                }
                else if (ex.Message.Contains("The operation has timed out."))
                {
                    //新增錯誤LOG
                    await _MQTTService.InsertMqttConnectionLogAsync(param, "連接無效，MQTT 操作已經超時");
                    return _responseService.normal<object>("", false, "IP：" + param.ip + "連接無效，MQTT 操作已經超時");
                }
                else
                {
                    //新增錯誤LOG
                    await _MQTTService.InsertMqttConnectionLogAsync(param, ex.Message);
                    return _responseService.normal<object>("", false, "IP：" + param.ip + ex.Message);
                }
            }
        }


        /// <summary>
        /// 獲取MQTT連線LOG訊息 / 瑋程 / 20231028
        /// </summary>
        [HttpGet("GetMqttConnectionLogMessages")]
        public async Task<ApiResponse<MqttConnectionLogMessageListView>> GetMqttConnectionLogMessagesAsync()
        {
            try
            {
                var data = await _MQTTService.GetMqttConnectionLogMessagesAsync();
                return _responseService.normal(data, true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal(new MqttConnectionLogMessageListView { }, false, ex.Message);
            }
        }

        /// <summary>
        /// 獲取蛋盤最新的位置 / 瑋程 / 20231030
        /// </summary>
        [HttpGet("GetEggTrayNewPositionData")]
        public async Task<ApiResponse<string>> GetEggTrayNewPositionDataAsync()
        {
            try
            {
                var data = await _MQTTService.GetEggTrayNewPositionDataAsync();
                return _responseService.normal(data, true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal("", false, ex.Message);
            }
        }

        /// <summary>
        /// 獲取蛋盤位置變更歷史訊息 / 瑋程 / 20231030
        /// </summary>
        [HttpGet("GetEggTrayPositionChangeHistoryMessage")]
        public async Task<ApiResponse<GetEggTrayPositionChangeHistoryMessageListView>> GetEggTrayPositionChangeHistoryMessageAsync()
        {
            try
            {
                var data = await _MQTTService.GetEggTrayPositionChangeHistoryMessageAsync();
                return _responseService.normal(data, true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal(new GetEggTrayPositionChangeHistoryMessageListView { }, false, ex.Message);
            }
        }

        /// <summary>
        /// 取得機械手臂放置蛋盤的位置 / 瑋程 / 20231101
        /// </summary>
        [HttpGet("GetEggTrayPositionThanRoboticArmPlace")]
        public async Task<ApiResponse<string>> GetEggTrayPositionThanRoboticArmPlaceAsync()
        {
            try
            {
                var data = await _MQTTService.GetEggTrayPositionThanRoboticArmPlaceAsync();
                return _responseService.normal(data, true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal("", false, ex.Message);
            }
        }







        /// <summary>   
        /// ❌ MQTT即時連線 / 瑋程 / 20230924
        /// </summary>
        [Obsolete("改用背景程式運行MQTT")]
        [HttpPost("Connect")]
        public async Task<ApiResponse<object>> ConnectToMQTTAsync([FromBody] ConnectParam param)
        {
            try
            {
                await _MQTTService.ConnectAsync(param);
                return _responseService.normal<object>("", true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal<object>("", false, ex.Message);
            }
        }


        /// <summary>
        /// ❌ MQTT即時發布訊息 / 瑋程 / 20230924 (如用到在檢查Service再使用)
        /// </summary>
        [Obsolete("改用背景程式運行MQTT")]
        [HttpPost("Publish")]
        public async Task<ApiResponse<object>> PublishAsync([FromBody] PublishParam param)
        {
            try
            {
                await _MQTTService.PublishAsync(param);
                return _responseService.normal<object>("", true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal<object>("", false, ex.Message);
            }
        }

        /// <summary>
        /// ❌ MQTT即時訂閱主題的訊息 / 瑋程 / 20230926
        /// </summary>
        [Obsolete("改用背景程式運行MQTT")]
        [HttpGet("SubscribeToTopic")]
        public async Task<ApiResponse<SubscribeReceiveMessageView>> SubscribeToTopicAsync([FromQuery] SubscribeToTopicParam param)
        {
            try
            {
                var data = await _MQTTService.SubscribeToTopicAsync(param);
                return _responseService.normal(data, true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal(new SubscribeReceiveMessageView { }, false, ex.Message);
            }
        }

        /// <summary>
        /// ❌ MQTT即時斷線 / 瑋程 / 20230927
        /// </summary>
        [Obsolete("改用背景程式運行MQTT")]
        [HttpPost("Disconnect")]
        public ApiResponse<object> Disconnect()
        {
            try
            {
                _MQTTService.Disconnect();
                return _responseService.normal<object>("", true, "");
            }
            catch (Exception ex)
            {
                return _responseService.normal<object>("", false, ex.Message);
            }
        }

        /// <summary>
        /// ❌ 歷史訊息 /  瑋程 / 20230927
        /// </summary>
        [Obsolete("改用背景程式運行MQTT")]
        [HttpGet("GetHistoryMessages")]
        public ApiResponse<List<string>> GetHistoryMessages()
        {
            return _responseService.normal(historyMessagesList.historyMessages, true, "");
        }

    }
}
