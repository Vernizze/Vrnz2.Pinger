using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.Settings;
using Vrnz2.Pinger.Shared.Interfaces;
using Vrnz2.Pinger.UseCases.SendPingEveryFiveSecs;

namespace Vrnz2.Pinger.UseCases.ScheduledAtEveryFiveSecs
{
    public class ScheduledAtEveryFiveSecs
    {
        #region Model

        public class Model
        {
            public class Input
                : IScheduleExecInputModel, INotification
            {
                public int IntervalInSecs { get; set; } = 5;
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
            : INotificationHandler<Model.Input>
        {
            #region Variables

            private readonly MessagesSettings _messageSettings;

            private readonly ILogger _logger;

            private readonly IMediator _mediator;

            private Timer _timer;

            #endregion

            #region Constructors

            public Handler(IOptions<MessagesSettings> messageSettingsOptions, ILogger logger, IMediator mediator) 
            {
                _messageSettings = messageSettingsOptions.Value;

                _logger = logger;

                _mediator = mediator;
            }

            #endregion

            public Task Handle(Model.Input notification, CancellationToken cancellationToken)
            {
                try
                {
                    Task.Run(() => Run(notification));

                    return Task.FromResult(new Model.Output
                    {
                        Success = true,
                        Message = $"Interval: {notification.IntervalInSecs} - Message: {_messageSettings.PingMessage}"
                    });
                }
                catch (Exception ex)
                {
                    var errorMessage = $"[SCHEDULE_START_ERROR] Error on to schedule message sending!!! Message: {ex.Message}";

                    _logger.Error(ex, errorMessage);

                    return Task.FromResult(new Model.Output
                    {
                        Success = false,
                        Message = errorMessage
                    });
                }
            }

            private void Run(Model.Input notification) 
                => _timer = new Timer(Send, new AutoResetEvent(false), 0, notification.IntervalInSecs * 1000);

            private void Send(Object stateInfo)
            {
                try
                {
                    Task.Run(() => _mediator.Publish(new SendPing.Model.Input
                    {
                        Message = _messageSettings.PingMessage
                    }));
                }
                catch (Exception ex)
                {
                    var errorMessage = $"[SCHEDULE_START_ERROR] Error on to schedule message sending!!! Message: {ex.Message}";

                    _logger.Error(ex, errorMessage);
                }
            }
        }

        #endregion
    }
}
