using AwsModel = Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Vrnz2.QueueHandler
{
    public delegate void EventDelegate(bool success, string msg);

    public class Queue
    {
        #region Variables

        private Timer _timer = null;

        #endregion

        #region Constructors

        public Queue(string queueUrl, string accessKey, string secretKey, string region, TimeSpan? recurringRunTime = null)
        {
            Url = queueUrl;

            Client = new Client(accessKey, secretKey, region);

            RecurringRunTime = recurringRunTime ?? TimeSpan.FromSeconds(2);

            _timer = new Timer(RecurringRunTime.TotalMilliseconds);
        }

        public Queue(string queueUrl, string region, TimeSpan? recurringRunTime = null)
        {
            Url = queueUrl;

            Client = new Client(region);

            RecurringRunTime = recurringRunTime ?? TimeSpan.FromSeconds(2);

            _timer = new Timer(RecurringRunTime.TotalMilliseconds);
        }

        public Queue(string queueUrl, Client client, TimeSpan? recurringRunTime = null)
        {
            Url = queueUrl;

            Client = client;

            RecurringRunTime = recurringRunTime ?? TimeSpan.FromSeconds(2);

            _timer = new Timer(RecurringRunTime.TotalMilliseconds);
        }

        #endregion

        #region Events

        public event EventDelegate SuccessEvent;
        public event EventDelegate ErrorEvent;

        #endregion

        #region Attributes

        public string Url { get; private set; } = string.Empty;

        public Client Client { get; private set; } = null;

        public TimeSpan RecurringRunTime { get; private set; }

        #endregion

        #region Methods

        //Send
        public async Task SendOne<T>(Message<T> message)
        {
            var request = new AwsModel.SendMessageRequest(Url, JsonConvert.SerializeObject(message.Body));

            var response = await Client.AwsClient.SendMessageAsync(request);

            if (HttpStatusCode.OK.Equals(response.HttpStatusCode))
                SuccessEvent?.Invoke(true, $"Message Sent Success => Message id : {response.MessageId} - Message MD5 Body: {response.MD5OfMessageBody}");
            else
                ErrorEvent?.Invoke(false, $"Message Sent Error => Message id : {response.MessageId} - Response Status Code: {response.HttpStatusCode}");
        }

        public async Task SendMany<T>(List<Message<T>> messages)
        {
            var request = new AwsModel.SendMessageBatchRequest
            {
                Entries = messages.Select(m => { return new AwsModel.SendMessageBatchRequestEntry(m.Id.ToString(), JsonConvert.SerializeObject(m.Body)); }).ToList(),
                QueueUrl = Url
            };

            var response = await Client.AwsClient.SendMessageBatchAsync(request);

            foreach (var success in response.Successful)
                SuccessEvent?.Invoke(true, $"Message Sent Success => Message id : {success.MessageId} - Message MD5 Body: {success.MD5OfMessageBody}");

            foreach (var failed in response.Failed)
                ErrorEvent?.Invoke(false, $"Message Sent Error => Message id : {failed.Id} - Message Body: {failed.Message}");
        }


        //Receive
        public void Receive<T>(Action<T> callBack)
        {
            _timer.Elapsed += async (Object source, ElapsedEventArgs e) =>
            {
                var request = new AwsModel.ReceiveMessageRequest();

                request.QueueUrl = Url;

                var receiveMessageResponse = await Client.AwsClient.ReceiveMessageAsync(Url);

#if DEBUG
                if (receiveMessageResponse?.Messages != null && receiveMessageResponse.Messages.Any())
                {
                    SuccessEvent?.Invoke(true, $"Messages Received => Messages Count : {receiveMessageResponse.Messages.Count} - Messages Bodies: {JsonConvert.SerializeObject(receiveMessageResponse.Messages)}");
                }
#endif

                foreach (var message in receiveMessageResponse.Messages)
                {
                    try
                    {
                        callBack(JsonConvert.DeserializeObject<T>(message.Body));

                        await DeleteMessage(message);
                    }
                    catch (Exception ex)
                    {
                        ErrorEvent?.Invoke(false, $"Message Receive Error => Message id : {message?.MessageId} - Message Body: {message?.Body} - Error: {ex.Message}-{ex.StackTrace}");
                    }
                }
            };

            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();
        }

        public void StopReceive()
        {
            _timer.Stop();
        }

        public void RestartReceive()
        {
            _timer.Start();
        }

        private async Task DeleteMessage(AwsModel.Message message)
        {
            var response = await Client.AwsClient.DeleteMessageAsync(new AwsModel.DeleteMessageRequest
            {
                QueueUrl = Url,
                ReceiptHandle = message.ReceiptHandle
            });

            if (!HttpStatusCode.OK.Equals(response.HttpStatusCode))
                ErrorEvent?.Invoke(false, $"Message Delete Error => Message id : {message.MessageId} - Status Code: {response.HttpStatusCode}");
        }

        #endregion
    }
}
