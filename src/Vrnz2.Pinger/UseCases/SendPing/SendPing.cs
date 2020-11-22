using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.Settings;
using Vrnz2.Pinger.Crosscutting.Shared.Handler;
using Vrnz2.Pinger.Crosscutting.Shared.Models;
using Vrnz2.Pinger.Crosscutting.Utils;
using Vrnz2.QueueHandler;

namespace Vrnz2.Pinger.UseCases.SendPingEveryFiveSecs
{
    public class SendPing
    {
        #region Model

        public class Model
        {
            public class Input
                : INotification
            {
                public string RequestId { get; set; }
                public string Message { get; set; }
            }
        }

        #endregion

        #region Handler

        public class Handler
            : INotificationHandler<Model.Input>
        {
            #region Variables

            private readonly AwsSqsSettings _awsSqsSettingsOptions;
            private readonly ServiceSettings _serviceSettings;
            private readonly ILogger _logger;

            #endregion

            #region Constructors

            public Handler(IOptions<AwsSqsSettings> awsSqsSettingsOptions, IOptions<ServiceSettings> serviceSettingsOptions, ILogger logger)
            {
                _awsSqsSettingsOptions = awsSqsSettingsOptions.Value;
                _serviceSettings = serviceSettingsOptions.Value;

                _logger = logger;
            }

            #endregion

            #region Methods

            public async Task Handle(Model.Input notification, CancellationToken cancellationToken)
            {
                try
                {
                    using (var queues = new QueuesPool(_awsSqsSettingsOptions.AccessKey, _awsSqsSettingsOptions.SecretKey, _awsSqsSettingsOptions.Region))
                    {
                        var queue = queues.AddQueue(_awsSqsSettingsOptions.QueuePingUrl);

                        await queue.SendOne(new Message<PingQueueMessage>(new PingQueueMessage
                        {
                            ServiceId = $"{ServiceInstanceHandler.Instance(_serviceSettings).GetServiceInstanceId}",
                            MessageId = notification.RequestId,
                            EventUnixTimestamp = DateTimeUtils.UnixTimestamp(),
                            Message = $"{notification.Message}"
                        }));
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error in when sending message! Message not sent: {notification.Message} - Error: {ex.Message}";

                    _logger.Error(ex, errorMessage);
                }
            }

            #endregion
        }

        #endregion
    }
}
