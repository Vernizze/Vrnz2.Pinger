using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vrnz2.Pinger
{
    class Program
    {
        static async Task Main(string[] args)
        {
            LogConfig.Config();
            Startup.ConfigureServices();
            
            var mediatr = Startup.GetService<IMediator>();

            var requestModels = Startup.GetScheduleExecInputModels();
            var notificationModels = Startup.GetListenQueueInputModels();

            requestModels.ForEach(m => mediatr.Publish(Activator.CreateInstance(m)));
            notificationModels.ForEach(m => mediatr.Send(Activator.CreateInstance(m)));

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
