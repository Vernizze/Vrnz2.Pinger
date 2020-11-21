using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.Settings;
using Vrnz2.Pinger.Crosscutting.Utils;
using Vrnz2.Pinger.Shared.Handler;
using Vrnz2.Pinger.Shared.Models;
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
            private readonly ILogger _logger;

            #endregion

            #region Constructors

            public Handler(IOptions<AwsSqsSettings> awsSqsSettingsOptions, ILogger logger)
            {
                _awsSqsSettingsOptions = awsSqsSettingsOptions.Value;

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
                        var queue = queues.AddQueue(_awsSqsSettingsOptions.QueueUrl);

                        await queue.SendOne(new Message<QueueMessage>(new QueueMessage
                        {
                            ServiceId = $"{ServiceInstanceHandler.Instance.GetServiceInstanceId}",
                            MessageId = Guid.NewGuid().ToString(),
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
