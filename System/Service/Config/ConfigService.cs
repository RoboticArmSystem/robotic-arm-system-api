using Microsoft.Extensions.Configuration;
using RoboticArmSystem.Core.BaseService;

namespace RoboticArmSystem.Core.Service.Config
{
    public class ConfigService : IService
    {
        private IConfiguration _configuration;

        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string get(string envKey)
        {
            return _configuration.GetValue<string>(envKey);
        }


        public string getConnectionString()
        {
            return get("ConnectionString");
        }

    }
}
