using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.Settings;
using Vrnz2.Pinger.Crosscutting.Utils;
using Vrnz2.Pinger.Shared.Interfaces;
using Vrnz2.Pinger.Shared.Models;
using Vrnz2.QueueHandler;

namespace Vrnz2.Pinger.UseCases.ListenPingEvents
{
    public class ListenPingEvents
    {
        #region Model

        public class Model
        {
            public class Input
                : IListenQueueInputModel, IRequest<Output>
            {
            }

            public class Output
            {
                public bool Success { get; set; }
                public string Message { get; set; }
            }
        }

        #endregion

        #region Handler

        public class Handler
            : IRequestHandler<Model.Input, Model.Output>
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

            public Task<Model.Output> Handle(Model.Input request, CancellationToken cancellationToken)
            {
                try
                {
                    var queues = new QueuesPool(_awsSqsSettingsOptions.AccessKey, _awsSqsSettingsOptions.SecretKey, _awsSqsSettingsOptions.Region);
                    var queue = queues.AddQueue(_awsSqsSettingsOptions.QueueUrl);

                    queue.ErrorEvent += ErrorEvent;

                    queue.Receive<QueueMessage>(ReceiveMessage);

                    return Task.FromResult(new Model.Output { Success = true, Message = $"The Queue {_awsSqsSettingsOptions.QueueUrl} has been started!" });
                }
                catch (Exception ex)
                {
                    var errorMessage = $"[MESSAGE_RECEIVED_ERROR] Error on to start receiving messages!!! Message: {ex.Message}";

                    _logger.Error(ex, errorMessage);

                    return Task.FromResult(new Model.Output { Success = false, Message = errorMessage });
                }
            }

            private void ReceiveMessage(QueueMessage message)
            {
                _logger.Information($"[MESSAGE_RECEIVED] ServiceId: {message.ServiceId} - MessageId: {message.MessageId} - Message Date/Time: {DateTimeUtils.UnixTimestamp(message.EventUnixTimestamp)} - Message: {message.Message}");
            }

            private void ErrorEvent(bool success, string message)
            {
                _logger.Information($"[MESSAGE_RECEIVED_ERROR] Error at message receiving!!! Message: {message}");
            }

            #endregion
        }

        #endregion
    }
}
