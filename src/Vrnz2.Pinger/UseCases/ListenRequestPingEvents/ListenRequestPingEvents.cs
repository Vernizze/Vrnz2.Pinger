using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.Settings;
using Vrnz2.Pinger.Crosscutting.Shared.Interfaces;
using Vrnz2.Pinger.Crosscutting.Shared.Models;
using Vrnz2.Pinger.UseCases.SendPingEveryFiveSecs;
using Vrnz2.QueueHandler;

namespace Vrnz2.Pinger.UseCases.ListenRequestPingEvents
{
    public class ListenRequestPingEvents
    {
        #region Model

        public class Model
        {
            public class Input
                : IListenQueuePingRequestInputModel, IRequest<Output>
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
            private readonly MessagesSettings _messageSettings;

            private readonly ILogger _logger;

            private readonly IMediator _mediator;

            #endregion

            #region Constructors

            public Handler(IOptions<AwsSqsSettings> awsSqsSettingsOptions, IOptions<MessagesSettings> messageSettingsOptions, ILogger logger, IMediator mediator)
            {
                _awsSqsSettingsOptions = awsSqsSettingsOptions.Value;
                _messageSettings = messageSettingsOptions.Value;

                _logger = logger;

                _mediator = mediator;
            }

            #endregion

            #region Methods

            public Task<Model.Output> Handle(Model.Input request, CancellationToken cancellationToken)
            {
                try
                {
                    var queues = new QueuesPool(_awsSqsSettingsOptions.AccessKey, _awsSqsSettingsOptions.SecretKey, _awsSqsSettingsOptions.Region);
                    var queue = queues.AddQueue(_awsSqsSettingsOptions.QueueRequestPingUrl);

                    queue.ErrorEvent += ErrorEvent;

                    queue.Receive<PingRequestQueueMessage>(ReceiveMessage);

                    return Task.FromResult(new Model.Output { Success = true, Message = $"The Queue {_awsSqsSettingsOptions.QueueRequestPingUrl} has been started!" });
                }
                catch (Exception ex)
                {
                    var errorMessage = $"[MESSAGE_RECEIVED_ERROR] Error on to start receiving messages!!! Message: {ex.Message}";

                    _logger.Error(ex, errorMessage);

                    return Task.FromResult(new Model.Output { Success = false, Message = errorMessage });
                }
            }

            private void ReceiveMessage(PingRequestQueueMessage message)
            {
                try
                {
                    _mediator.Publish(new SendPing.Model.Input
                    {
                        RequestId = message.RequestId,
                        Message = _messageSettings.PingMessage
                    });
                }
                catch (Exception ex)
                {
                    _logger.Information($"[MESSAGE_RECEIVED_ERROR] Error at message receiving!!! Message: {message}");
                }
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
