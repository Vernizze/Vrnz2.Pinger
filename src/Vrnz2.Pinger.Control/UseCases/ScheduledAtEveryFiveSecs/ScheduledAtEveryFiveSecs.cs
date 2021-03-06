﻿using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Pinger.Crosscutting.Settings;
using Vrnz2.Pinger.Crosscutting.Shared.Interfaces;
using Vrnz2.Pinger.Crosscutting.Shared.Models;
using Vrnz2.Pinger.Crosscutting.Utils;
using Vrnz2.QueueHandler;

namespace Vrnz2.Pinger.Control.UseCases.ScheduledAtEveryFiveSecs
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
            private readonly AwsSqsSettings _awsSqsSettings;

            private readonly ILogger _logger;

            private readonly IMediator _mediator;

            private Timer _timer;

            #endregion

            #region Constructors

            public Handler(IOptions<MessagesSettings> messageSettingsOptions, IOptions<AwsSqsSettings> awsSqsSettingsOptions, ILogger logger, IMediator mediator) 
            {
                _messageSettings = messageSettingsOptions.Value;
                _awsSqsSettings = awsSqsSettingsOptions.Value;

                _logger = logger;

                _mediator = mediator;
            }

            #endregion

            public Task Handle(Model.Input notification, CancellationToken cancellationToken)
                => Task.Run(() => Run(notification));

            private void Run(Model.Input notification)
                => _timer = new Timer(Send, new AutoResetEvent(false), 0, notification.IntervalInSecs * 1000);

            private void Send(Object stateInfo)
            {
                try
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            using (var queues = new QueuesPool(_awsSqsSettings.AccessKey, _awsSqsSettings.SecretKey, _awsSqsSettings.Region))
                            {
                                var queue = queues.AddQueue(_awsSqsSettings.QueueRequestPingUrl);

                                await queue.SendOne(new Message<PingRequestQueueMessage>(new PingRequestQueueMessage
                                {
                                    RequestId = Guid.NewGuid().ToString(),
                                    TimeStamp = DateTimeUtils.UnixTimestamp()
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMessage = $"[SCHEDULE_START_ERROR] Error on to schedule message sending!!! Message: {ex.Message}";

                            _logger.Error(ex, errorMessage);
                        }
                    });
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
