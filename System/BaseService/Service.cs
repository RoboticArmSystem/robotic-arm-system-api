using System;

namespace RoboticArmSystem.Core.BaseService
{
    public abstract class Service : IService
    {
        protected Service(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
