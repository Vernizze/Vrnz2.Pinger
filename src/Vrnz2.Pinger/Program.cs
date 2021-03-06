﻿using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.StartupHandlers;

namespace Vrnz2.Pinger
{
    class Program
    {
        static async Task Main(string[] args)
        {
            LogConfig.Config();
            Startup.ConfigureServices();
            
            var mediatr = Startup.GetService<IMediator>();

            Startup.GetListenQueuePingRequestInputModels()
                .ForEach(m => mediatr.Send(Activator.CreateInstance(m)));

            Startup.GetListenQueuePingInputModels()
                .ForEach(m => mediatr.Send(Activator.CreateInstance(m)));

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
