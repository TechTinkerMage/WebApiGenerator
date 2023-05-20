using Microsoft.Extensions.DependencyInjection;
using WebApiGenerator.Helpers;
using WebApiGenerator.Interfaces;
using WebApiGenerator.ViewModels;

namespace WebApiGenerator.IoC
{
    public class DIContainer
    {
        private static readonly ServiceProvider _serviceProvider;

        static DIContainer()
        {
            var services = new ServiceCollection();

            services.Scan(scan =>
                  scan.FromAssembliesOf(typeof(IService))
                      .AddClasses(classes => classes.AssignableTo<ISingletonService>())
                          .AsSelf()
                          .AsImplementedInterfaces().WithSingletonLifetime()
                      .AddClasses(classes => classes.AssignableTo<IScopedService>())
                          .AsSelf()
                          .AsImplementedInterfaces().WithScopedLifetime()
                      .AddClasses(classes => classes.AssignableTo<IService>())
                          .AsSelf()
                          .AsImplementedInterfaces().WithTransientLifetime());

            _serviceProvider = services.BuildServiceProvider();
        }

        public static T Resolve<T>() where T : notnull => _serviceProvider.GetService<T>();
    }
}
