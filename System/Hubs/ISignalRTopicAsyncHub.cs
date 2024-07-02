using RoboticArmSystem.Core.BaseService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmSystem.Core.Hubs
{
    public interface ISignalRTopicAsyncHub
    {
        Task EggTrayPositionData(string param);
        Task SetMqttConnectionStatus(string isParam);
        Task EggTaryPositionMessage(string param);

        Task SystemRunning(string state);
        Task ArmState(string state);
        Task AIModal(string state);
        Task ARMEnd(string state);
    }
}
