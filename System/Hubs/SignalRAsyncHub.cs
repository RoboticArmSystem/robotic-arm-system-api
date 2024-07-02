using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmSystem.Core.Hubs
{
    public class SignalRAsyncHub: Hub<ISignalRTopicAsyncHub>
    {
        /// MQTT連線狀態
        private string isMqttConnected = "false";

        /// <summary>
        /// 雞蛋位於蛋盤上位置
        /// </summary>
        [HubMethodName("EggTrayPositionData")]
        public async Task EggTrayPositionData(string mes)
        {
            // 傳遞資料格式 1,1,1,1,1
            await Clients.All.EggTrayPositionData(mes);
        }

        /// <summary>
        /// MQTT連線狀態 (無使用) 
        /// </summary>
        [HubMethodName("SetMqttConnectionStatus")]
        public async Task SetMqttConnectionStatus(string isConnected)
        {
            isMqttConnected = isConnected;
            await Clients.All.SetMqttConnectionStatus(isMqttConnected);
        }

        /// <summary>
        /// 蛋盤某位置雞蛋的移出移入訊息通知 
        /// </summary>
        [HubMethodName("EggTaryPositionMessage")]
        public async Task EggTaryPositionMessage(string mes)
        {
            await Clients.All.EggTaryPositionMessage(mes);
        }

        [HubMethodName("SystemRunning")]
        public async Task SystemRunningAsync(string state)
        {
            await Clients.All.SystemRunning(state);
        }

        // 推播機器手臂狀態
        [HubMethodName("ArmState")]
        public async Task ArmStateAsync(string state)
        {
            await Clients.All.ArmState(state);
        }

        // AI流程圖示推播
        [HubMethodName("AIModal")]
        public async Task AIModalAsync(string state)
        {
            await Clients.All.AIModal(state);
        }

        // 機械手臂結束流程圖示推播
        [HubMethodName("ARMEnd")]
        public async Task ARMEndAsync(string state)
        {
            await Clients.All.ARMEnd(state);
        }


    }
}
