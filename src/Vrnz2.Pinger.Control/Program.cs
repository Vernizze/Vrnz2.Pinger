using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.StartupHandlers;

namespace Vrnz2.Pinger.Control
{
    class Program
    {
        static async Task Main(string[] args)
        {
            LogConfig.Config();
            Startup.ConfigureServices();

            var mediatr = Startup.GetService<IMediator>();

            Startup.GetScheduleExecInputModels()
                .ForEach(m => mediatr.Publish(Activator.CreateInstance(m)));

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
