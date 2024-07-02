using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using RoboticArmSystem.Core.Service.Config;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using RoboticArmSystem.Core.Utils;
using System.Linq;
using RoboticArmSystem.Core.Service.MQTTBackgroundService.Models.Views;
using RoboticArmSystem.Core.Service.MqttSetting.Models.Params.Repo;
using RoboticArmSystem.Core.Service.MqttSetting.Models.Views;
using RoboticArmSystem.Core.Service.MqttSetting.Models.Params;

namespace RoboticArmSystem.Core.Service.MqttSetting
{
    public class MQTTService : IMQTTService
    {
        // 即時MQTT
        private readonly IMqttClient _mqttClient;
        private static List<string> _historicalMessage = new List<string>();
        private readonly SqlConnection _conn;

        public MQTTService(MqttFactory mqttFactory, ConfigService configService)
        {
            _mqttClient = mqttFactory.CreateMqttClient();
            _conn = new SqlConnection(configService.getConnectionString());
        }

        #region Service

        public async Task ConnectAsync(ConnectParam param)
        {

            var connectResult = await _mqttClient.ConnectAsync(clientOptions(param.ipAddress, int.Parse(param.port)));

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // 連線成功發布消息
                // 傳送 MQTT 訊息
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(param.topic) // 指定 MQTT 主題
                    .WithPayload(param.topic + "連線成功") // 使用從前端接收到的訊息
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(mqttMessage);
            }
            else
            {
                throw new Exception("連線失敗");
            }
        }


        public async Task PublishAsync(PublishParam param)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(param.ipAddress, int.Parse(param.port))
                .WithCleanSession()
                .Build();

            var connectResult = await _mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // 連線成功發布消息
                // 傳送 MQTT 訊息
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(param.topic) // 指定 MQTT 主題
                    .WithPayload(param.message) // 使用從前端接收到的訊息
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(mqttMessage);
            }
            else
            {
                throw new Exception("訊息發布失敗");
            }
        }


        public async Task<SubscribeReceiveMessageView> SubscribeToTopicAsync(SubscribeToTopicParam param)
        {

            var connectResult = await _mqttClient.ConnectAsync(clientOptions(param.ipAddress, int.Parse(param.port)));

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // Subscribe to a topic
                await _mqttClient.SubscribeAsync(param.topic);

                string tmpmes = "";
                // 監聽訊息直到有訊息跳出迴圈
                while (tmpmes == "")
                {
                    // Callback function when a message is received
                    _mqttClient.ApplicationMessageReceivedAsync += e =>
                    {
                        tmpmes = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                        return Task.CompletedTask;
                    };
                }
                _historicalMessage.Add(tmpmes);
            }

            return new SubscribeReceiveMessageView { receiveMessag = _historicalMessage };
        }

        public void Disconnect()
        {
            _historicalMessage.Clear();
        }

        public async Task<GetMqttConnectionInfoView> GetMqttConnectionInfoAsync()
        {
            await _conn.OpenAsync();
            var result = await MqttConnectionInfoRepoAsync();
            await _conn.CloseAsync();
            return result;

        }

        public async Task UpdateMqttConnectionInfoAsync(UpdateMQTTConnectionInfoParam param)
        {
            await _conn.OpenAsync();
            using (var transaction = _conn.BeginTransaction())
            {
                try
                {
                    await UpdateMQTTConnectionInfoRepoAsync(new UpdateMQTTConnectionInfoParam
                    {
                        ip = param.ip,
                        port = "1883",
                        topic = param.topic
                    }, transaction);

                    transaction.Commit();
                    await _conn.CloseAsync();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    await _conn.CloseAsync();
                    throw new Exception("儲存失敗：" + ex.Message);
                }
            }

        }

        public async Task InsertMqttConnectionLogAsync(UpdateMQTTConnectionInfoParam param, string status)
        {
            await _conn.OpenAsync();
            using (var transaction = _conn.BeginTransaction())
            {
                try
                {
                    await InsertMqttConnectionLogRepoAsync(new InsertMqttConnectionLogRepoParam
                    {
                        createtime = GetTimeTool.GetNowDate(),
                        ip = param.ip,
                        topic = param.topic,
                        message = "IP：" + param.ip + status
                    }, transaction); ;
                    transaction.Commit();
                    await _conn.CloseAsync();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    await _conn.CloseAsync();
                    throw new Exception("Log寫入錯誤：" + ex.Message);
                }
            }



        }

        public async Task<MqttConnectionLogMessageListView> GetMqttConnectionLogMessagesAsync()
        {
            await _conn.OpenAsync();
            var result = await MqttConnectionLogMessagesRepoAsync();
            await _conn.CloseAsync();

            return result;
        }

        public async Task<string> GetEggTrayNewPositionDataAsync()
        {
            await _conn.OpenAsync();
            var result = await GetEggTrayNewPositionDataRepoAsync();
            await _conn.CloseAsync();
            string tmpAddPosition = "";

            if (result != null)
            {
                bool isFrist = true;
                for (int i = 0; i < 5; i++)
                {
                    if (isFrist)
                    {
                        tmpAddPosition += result.GetType().GetProperty("p" + (i + 1)).GetValue(result).ToString();
                        isFrist = false;
                    }
                    else
                    {
                        tmpAddPosition += "," + result.GetType().GetProperty("p" + (i + 1)).GetValue(result).ToString();
                    }

                }
            }

            return tmpAddPosition;
        }

        public async Task<GetEggTrayPositionChangeHistoryMessageListView> GetEggTrayPositionChangeHistoryMessageAsync()
        {
            await _conn.OpenAsync();
            var result = await GetEggTrayPositionChangeHistoryMessageRepoAsync();
            await _conn.CloseAsync();

            return result;
        }

        public async Task<string> GetEggTrayPositionThanRoboticArmPlaceAsync()
        {

            await _conn.OpenAsync();
            var result = await GetEggTrayPositionThanRoboticArmPlaceRepoAsync();
            await _conn.CloseAsync();

            string tmpPosition = "0";
            if (result != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (result.GetType().GetProperty("p" + (i + 1)).GetValue(result).ToString() == "0")
                    {
                        tmpPosition = (i + 1).ToString();
                        break;
                    }
                }
                return tmpPosition;
            }
            else
            {
                return tmpPosition = "0";
            }


        }

        #endregion

        #region Method

        private MqttClientOptions clientOptions(string ipAddress, int port)
        {
            var options = new MqttClientOptionsBuilder()
               .WithTcpServer(ipAddress, port) // MQTT broker address and port
               .WithCleanSession()
               .Build();

            return options;
        }

        #endregion

        #region Repo

        private async Task<GetMqttConnectionInfoView> MqttConnectionInfoRepoAsync()
        {
            string sql = $@"SELECT * FROM MqttInfo";

            var data = await _conn.QueryFirstAsync<GetMqttConnectionInfoView>(sql, null);

            return data;
        }

        private async Task UpdateMQTTConnectionInfoRepoAsync(UpdateMQTTConnectionInfoParam param, SqlTransaction? transaction)
        {
            string sql = $@"UPDATE
                                MqttInfo
                            SET 
                                ip = @ip,
                                topic = @topic
                            WHERE 
                                port = @port";

            await _conn.ExecuteAsync(sql, param, transaction: transaction);
        }

        private async Task InsertMqttConnectionLogRepoAsync(InsertMqttConnectionLogRepoParam param, SqlTransaction? transaction)
        {
            string sql = $@"INSERT INTO
                                MqttConnectionLog
                                (createtime,
                                ip,
                                topic,
                                message)
                            VALUES
                                (@createtime,
                                @ip,
                                @topic,
                                @message)";

            await _conn.ExecuteAsync(sql, param, transaction: transaction);
        }

        private async Task<MqttConnectionLogMessageListView> MqttConnectionLogMessagesRepoAsync()
        {
            string sql = $@"SELECT
                                FORMAT(createtime, 'yyyy-MM-dd HH:mm:ss') createtime,
                                ip,
                                topic,
                                message
                            FROM 
                                MqttConnectionLog
                            ORDER BY
	                            createtime";
            var data = await _conn.QueryAsync<MqttConnectionLogMessageView>(sql, null);

            return new MqttConnectionLogMessageListView { mqttConnectionLogMessageViews = data.ToList() };
        }

        private async Task<GetLastEggTrayPositionDataView> GetEggTrayNewPositionDataRepoAsync()
        {
            string sql = $@"SELECT 
                                TOP 1 *
                            FROM 
                                EggTrayData
                            ORDER BY 
                                createtime DESC;";

            var data = await _conn.QueryFirstAsync<GetLastEggTrayPositionDataView>(sql, null);

            return data;
        }

        private async Task<GetEggTrayPositionChangeHistoryMessageListView> GetEggTrayPositionChangeHistoryMessageRepoAsync()
        {
            string sql = $@"SELECT 
　                            pId,
　                            positionChange,
　                            createtime
                            FROM 
　                            EggTrayData
                            ORDER BY 
                                createtime";

            var data = await _conn.QueryAsync<GetEggTrayPositionChangeHistoryMessageView>(sql, null);

            return new GetEggTrayPositionChangeHistoryMessageListView { positionChangeHistoryMessageViews = data.ToList() };
        }

        private async Task<GetLastEggTrayPositionDataView> GetEggTrayPositionThanRoboticArmPlaceRepoAsync()
        {
            string sql = $@"SELECT 
                                TOP 1 *
                            FROM 
                                EggTrayData
                            ORDER BY 
                                createtime DESC;";

            var data = await _conn.QueryFirstAsync<GetLastEggTrayPositionDataView>(sql, null);

            return data;
        }

        #endregion
    }
}
