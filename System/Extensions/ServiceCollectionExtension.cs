using Microsoft.Extensions.DependencyInjection;
using RoboticArmSystem.Core.BaseService;
using RoboticArmSystem.Core.Extensions;
using System;
using System.Linq;

namespace RoboticArmSystem.Core.Extensions
{
    public static class ServiceCollectionExtension
    {
        private static IServiceCollection AddAllTypes<T>(this IServiceCollection services, ServiceLifetime lifetime)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T)) && !x.IsInterface && !x.IsAbstract)).ToList();
            foreach (var type in types)
            {
                services.Add(new ServiceDescriptor(type, type, lifetime));

                var interfaces = type.GetInterfaces().ToList();
                foreach (var @interface in interfaces)
                {
                    services.Add(new ServiceDescriptor(@interface, sp => sp.GetRequiredService(type), lifetime));
                }
            }
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddAllTypes<IService>(ServiceLifetime.Transient);
            return services;
        }
    }
}
