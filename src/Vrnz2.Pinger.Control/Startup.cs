using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vrnz2.Pinger.Crosscutting.Settings;
using Vrnz2.Pinger.Crosscutting.Shared.Interfaces;

namespace Vrnz2.Pinger.Control
{
    public static class Startup
    {
        #region Attributes

        private static ServiceProvider GetServiceProvider { get; set; }

        private static IConfiguration GetSettings
            => new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        private static Assembly GetAssemblies
            => typeof(Startup)
            .GetTypeInfo()
            .Assembly;

        #endregion

        #region Methods

        public static IServiceCollection ConfigureServices()
            => new ServiceCollection()
                    .AddMediatR(GetAssemblies)
                    .AddSettings(GetSettings)
                    .AddSingleton(_ => Log.Logger.ForContext<ILogger>())
                    .AddIServiceColletion()
                    .MakeServiceProvider();

        public static T GetService<T>()
            => GetServiceProvider.GetService<T>();

        private static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
            => services
                .Configure<MessagesSettings>(configuration.GetSection("MessagesSettings"))
                .Configure<AwsSqsSettings>(configuration.GetSection("AwsSqsSettings"))
                .Configure<ServiceSettings>(configuration.GetSection("ServiceSettings"));

        private static IServiceCollection AddIServiceColletion(this IServiceCollection services)
            => services.AddSingleton(services);

        private static IServiceCollection MakeServiceProvider(this IServiceCollection services)
        {
            GetServiceProvider = services.BuildServiceProvider();

            return services;
        }

        public static List<Type> GetScheduleExecInputModels()
        {
            var models = new List<Type>();

            foreach (Type type in GetAssemblies.GetTypes())
            {
                if (type.GetInterfaces().Contains(typeof(IScheduleExecInputModel)))
                    models.Add(type);
            }

            return models;
        }

        #endregion
    }
}
