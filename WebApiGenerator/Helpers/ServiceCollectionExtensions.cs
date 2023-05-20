using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebApiGenerator.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices<T>(this IServiceCollection services, Func<ILifetimeSelector, IImplementationTypeSelector> lifetimeSelector)
        {
            services.Scan(scan =>
                scan.FromAssembliesOf(typeof(T))
                    .AddClasses(classes => classes.AssignableTo<T>())
                    .AsImplementedInterfaces());
            return services;
        }
    }
}
