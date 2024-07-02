using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MQTTnet;
using MQTTnet.Client;
using RoboticArmSystem.Core.Hubs;
using RoboticArmSystem.Core.Hubs.Modules;
using RoboticArmSystem.Core.Service.Config;
using RoboticArmSystem.Core.Service.MQTTBackgroundService.Models.Params;
using RoboticArmSystem.Core.Service.MQTTBackgroundService.Models.Views;
using RoboticArmSystem.Core.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboticArmSystem.Core.Service.MQTTBackgroundService
{
    public class MQTTBackgroundService
    {
        private readonly SqlConnection _conn;
        private static MqttFactory _mqttFactory = new MqttFactory();
        private IMqttClient _mqttClient;
        private SignalRClient _signalRClient;
        public bool SetMqttConnectionStatus = false;
        //紀錄儲存主題(用於解綁定)
        private string _subscribedTopic = "";

        public MQTTBackgroundService(ConfigService configService)
        {
            _signalRClient = new SignalRClient(configService.get("SignalRUrl"));
            _mqttClient = _mqttFactory.CreateMqttClient();
            _conn = new SqlConnection(configService.getConnectionString());
            GetServerConnectAsync().Wait();
        }

        #region Service

        /// <summary>
        /// 取連線資料後，進行MQTT連線及訂閱 / 瑋程 / 20231024
        /// </summary>
        private async Task GetServerConnectAsync()
        {
            try
            {
                await _conn.OpenAsync();
                var data = await GetServerConnectRepoAsync();
                await _conn.CloseAsync();

                if (data != null)
                {
                    ServerConnectAsync(data.ip).Wait();
                    SubcribeTopicAsync(data.topic).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("資料庫錯誤：" + ex.Message);
            }
        }

        /// <summary>
        /// MQTT 初始連線與訊息接收推播及訊息存儲 / 瑋程 / 20230929
        /// </summary>
        public async Task ServerConnectAsync(string ip)
        {
            try
            {

                var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(ip).Build();
                var response = await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                if (response.ResultCode == MqttClientConnectResultCode.Success)
                {
                    SetMqttConnectionStatus = true;
                }
                else
                {
                    SetMqttConnectionStatus = false;
                }


                _mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    string topic = e.ApplicationMessage.Topic;

                    if (topic == "eggplate/start")
                    {
                        Console.WriteLine(topic + "：" + Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));

                        string[] strSplit = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment).Split(',');
                        // 紀錄雞蛋移出或移入
                        string tmpPositionChanges = "";

                        if (strSplit.Length == 5)
                        {
                            await _conn.OpenAsync();

                            // 獲取蛋盤最後位置
                            var result = await GetLastEggTrayDataRepoAsync();

                            if (result != null)
                            {
                                // 比較每個位置的值
                                for (int i = 0; i < strSplit.Length; i++)
                                {
                                    int lastValue = int.Parse(result.GetType().GetProperty("p" + (i + 1)).GetValue(result).ToString());
                                    int currentValue = int.Parse(strSplit[i]);

                                    if (lastValue > currentValue)
                                    {
                                        // 執行移出操作
                                        tmpPositionChanges = $"蛋盤位置 {i + 1} 有雞蛋移出";
                                    }
                                    else if (lastValue < currentValue)
                                    {
                                        // 執行放入操作
                                        tmpPositionChanges = $"蛋盤位置 {i + 1} 有雞蛋放入";
                                    }
                                }

                            }
                            else
                            {
                                //第一次加入
                                bool isFirst = true; // 判斷是否是第一個符合條件的字串
                                for (int i = 0; i < strSplit.Length; i++)
                                {
                                    if (strSplit[i] == "1")
                                    {
                                        if (isFirst)
                                        {
                                            tmpPositionChanges += $"蛋盤位置 {i + 1} 有雞蛋放入";
                                            isFirst = false;
                                        }
                                        else
                                        {
                                            tmpPositionChanges += $",蛋盤位置 {i + 1} 有雞蛋放入";
                                        }
                                    }
                                }
                            }

                            using (var transaction = _conn.BeginTransaction())
                            {
                                try
                                {
                                    var id = GuidGenerator.GetNewGuid().ToString();
                                    var nowtime = GetTimeTool.GetNowDate();

                                    await InsertEggTrayDataRepoAsync(new InsertEggTrayDataRepoParam
                                    {
                                        pId = id,
                                        p1 = strSplit[0],
                                        p2 = strSplit[1],
                                        p3 = strSplit[2],
                                        p4 = strSplit[3],
                                        p5 = strSplit[4],
                                        positionChange = tmpPositionChanges,
                                        createtime = nowtime
                                    }, transaction);
                                    transaction.Commit();
                                    await _conn.CloseAsync();



                                    // signalR訊息推播
                                    _signalRClient.startSignalRConnect();

                                    _signalRClient.Proxy.InvokeAsync("EggTrayPositionData", Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)).Wait();

                                    _signalRClient.Proxy.InvokeAsync("EggTaryPositionMessage", tmpPositionChanges).Wait();

                                    _signalRClient.stopSignalRConnect();
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    await _conn.CloseAsync();
                                    Console.WriteLine("新增錯誤：" + ex.Message);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("資料格式錯誤，數量不足５個");
                        }
                    }
                    else if(topic == "ArmSystem")
                    {
                        
                        Console.WriteLine(topic + "：" + Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));
                        string[] strSplit = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment).Split(',');
                        string ff = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                        if (strSplit[1] == "over")
                        {
                            
                            // 推播超出範圍
                            _signalRClient.startSignalRConnect();

                            _signalRClient.Proxy.InvokeAsync("ArmState", strSplit[0] + "號雞蛋" + "超出範圍").Wait();

                            _signalRClient.stopSignalRConnect();
                            Console.WriteLine(strSplit[0] + "號雞蛋" + "超出範圍");
                        }
                        else
                        {
                            string[] tmpstrSplit = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment).Split(',');
                            // 推播編號，X軸，Y軸，距離
                            string tmp =  tmpstrSplit[0] + "號雞蛋";
                            tmp += " x值:" + tmpstrSplit[1];
                            tmp += " y值:" + tmpstrSplit[2];
                            string formattedValue = "";
                            if (double.TryParse(tmpstrSplit[3], out double floatValue))
                            {
                                formattedValue = floatValue.ToString("0.00");
                            }
                            tmp += " 距離:" + formattedValue;

                            _signalRClient.startSignalRConnect();

                            _signalRClient.Proxy.InvokeAsync("ArmState", tmp).Wait();

                            _signalRClient.stopSignalRConnect();

                            Console.WriteLine(tmp);
                        }
                      

                    }
                    else if (topic == "ArmEnd")
                    {
                        // 推播機械手臂動作結束
                        if (Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment) == "ARM_over")
                        {
                            _signalRClient.startSignalRConnect();

                            _signalRClient.Proxy.InvokeAsync("ArmState", "End").Wait();

                            _signalRClient.stopSignalRConnect();
                        }
                    }

                   



                    //return Task.CompletedTask;
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("錯誤訊息：" + ex.Message);
            }


        }

        /// <summary>
        /// MQTT 訂閱單個主題 / 瑋程 / 20230929
        /// </summary>
        public async Task SubcribeTopicAsync(string topic)
        {
            try
            {
                var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(topic);
                    })
                .Build();

                var mqttSubscribeOptions2 = _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("ArmSystem");
                    })
                .Build();

                _subscribedTopic = topic;

                await _mqttClient.SubscribeAsync(mqttSubscribeOptions);
                await _mqttClient.SubscribeAsync(mqttSubscribeOptions2);
                Console.WriteLine(topic + " 訂閱成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("錯誤訊息：" + ex.Message);
            }

        }

        /// <summary>
        /// 解除原MQTT連線，綁定新的MQTT連線資料 / 瑋程 / 20231026
        /// </summary>
        public async Task UpdateMqttConnectionInfoAsync(string newIp, string newTopic)
        {
            // 關閉現有的MQTT連線
            await _mqttClient.DisconnectAsync();

            // 更新IP
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(newIp).Build();
            var response = await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            if (response.ResultCode == MqttClientConnectResultCode.Success)
            {
                SetMqttConnectionStatus = true;
            }
            else
            {
                SetMqttConnectionStatus = false;
            }

            // 取消已訂閱主題
            await _mqttClient.UnsubscribeAsync(_subscribedTopic);

            // 更新訂閱新主题
            await SubcribeTopicAsync(newTopic);
        }

        #endregion


        #region Repo

        private async Task<GetServerConnectView> GetServerConnectRepoAsync()
        {
            string sql = $@"SELECT 
	                            ip,
	                            port,
	                            topic
                            FROM
	                            MqttInfo";

            var result = await _conn.QueryFirstAsync<GetServerConnectView>(sql, null);

            return result;
        }

        private async Task InsertEggTrayDataRepoAsync(InsertEggTrayDataRepoParam param, SqlTransaction? transaction)
        {
            string sql = $@"INSERT INTO
                                EggTrayData
                                    (pId,
                                    p1,
                                    p2,
                                    p3,
                                    p4,
                                    p5,
                                    positionChange,
                                    createtime)
                                VALUES
                                    (@pId,
                                    @p1,
                                    @p2,
                                    @p3,
                                    @p4,
                                    @p5,
                                    @positionChange,
                                    @createtime)";


            await _conn.ExecuteAsync(sql, param, transaction: transaction);
        }

        private async Task<GetLastEggTrayPositionDataView> GetLastEggTrayDataRepoAsync()
        {
            string sql = $@"SELECT 
                                TOP 1 *
                            FROM 
                                EggTrayData
                            ORDER BY 
                                createtime DESC;";

            var data = await _conn.QueryFirstOrDefaultAsync<GetLastEggTrayPositionDataView>(sql, null);

            return data;
        }

        #endregion
    }
}
