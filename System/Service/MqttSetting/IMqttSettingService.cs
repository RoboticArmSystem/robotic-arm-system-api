using RoboticArmSystem.Core.BaseService;
using RoboticArmSystem.Core.Service.MQTTBackgroundService.Models.Views;
using RoboticArmSystem.Core.Service.MqttSetting.Models.Params;
using RoboticArmSystem.Core.Service.MqttSetting.Models.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmSystem.Core.Service.MqttSetting
{
    public interface IMQTTService : IService
    {
        Task ConnectAsync(ConnectParam param);

        Task PublishAsync(PublishParam param);

        Task<SubscribeReceiveMessageView> SubscribeToTopicAsync(SubscribeToTopicParam param);

        void Disconnect();

        Task<GetMqttConnectionInfoView> GetMqttConnectionInfoAsync();

        Task UpdateMqttConnectionInfoAsync(UpdateMQTTConnectionInfoParam param);

        Task InsertMqttConnectionLogAsync(UpdateMQTTConnectionInfoParam param, string status);

        Task<MqttConnectionLogMessageListView> GetMqttConnectionLogMessagesAsync();

        Task<string> GetEggTrayNewPositionDataAsync();

        Task<GetEggTrayPositionChangeHistoryMessageListView> GetEggTrayPositionChangeHistoryMessageAsync();

        Task<string> GetEggTrayPositionThanRoboticArmPlaceAsync();
    }
}
