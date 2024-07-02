using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmSystem.Core.Hubs.Modules
{
    public class SignalRClient
    {
        /// <summary>
        /// signalR 連線物件
        /// </summary>f
        private HubConnection _hubConn;

        /// <summary>
        /// 提供給實例物件使用之 HubConnection 變數
        /// </summary>
        public HubConnection Proxy
        {
            get { return _hubConn; }
            private set { }
        }

        /// <summary>
        /// 建立 signalR 連線物件 / 瑋程(by 陳建升) / 20231026
        /// </summary>
        public SignalRClient(string serverUrl)
        {
            _hubConn = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .Build();

            this.subscribeHubMethods();
        }

        private void subscribeHubMethods()
        {
            _hubConn.On<string>("EggTrayPositionData", (message) =>
            {
                Console.WriteLine("EggTrayPositionData message: " + message);
            });
            
            _hubConn.On<string>("SetMqttConnectionStatus", (message) =>
            {
                Console.WriteLine("SetMqttConnectionStatus message: " + message);
            }); 
            
            _hubConn.On<string>("EggTaryPositionMessage", (message) =>
            {
                Console.WriteLine("EggTaryPositionMessage message: " + message);
            });


        }

        /// <summary>
        /// 建立 signalR 連線並告知 server 已連線 / 瑋程(by 陳建升) / 20231026
        /// </summary>
        public void startSignalRConnect()
        {
            _hubConn.StartAsync().Wait();
            //_hubConn.StartAsync();
        }

        /// <summary>
        /// 關閉 signalR 連線 / 瑋程(by 陳建升) / 20231026
        /// </summary>
        public void stopSignalRConnect()
        {
            _hubConn.StopAsync().Wait();
            //_hubConn.StopAsync();
        }
    }
}
