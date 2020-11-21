using System;
using System.Collections.Generic;
using System.Linq;

namespace Vrnz2.QueueHandler
{
    public class QueuesPool
        : IDisposable
    {
        #region Constructors

        private readonly string _accessKey = string.Empty;
        private readonly string _secretKey = string.Empty;
        private readonly string _region = string.Empty;

        #endregion

        #region Constructors

        public QueuesPool(string accessKey, string secretKey, string region)
        {
            _accessKey = accessKey;
            _secretKey = secretKey;
            _region = region;
        }

        public QueuesPool(string region)
            => _region = region;

        #endregion

        #region Attributes

        public Client Client { get; private set; } = null;

        public Dictionary<string, Queue> Queues { get; private set; } = new Dictionary<string, Queue>();

        public List<Queue> QueuesList => Queues.Select(q => q.Value).ToList();

        #endregion

        #region Methods

        public void Dispose()
        {
            Queues.Clear();

            Client?.Dispose();
        }

        public Queue AddQueue(string queueUrl)
        {
            Queue queue;

            if (Queues.Any())
            {
                queue = new Queue(queueUrl, Client);
            }
            else
            {
                queue = !string.IsNullOrEmpty(_accessKey) && !string.IsNullOrEmpty(_secretKey) ?
                        new Queue(queueUrl, _accessKey, _secretKey, _region) :
                        new Queue(queueUrl, _region);

                Client = queue.Client;
            }

            Queues.Add(queueUrl, queue);

            return queue;
        }

        public Queue GetQueue(string queueUrl)
        {
            Queues.TryGetValue(queueUrl, out Queue queue);

            return queue;
        }

        public void RemoveQueue(string queueUrl)
        {
            if (Queues.TryGetValue(queueUrl, out _))
                Queues.Remove(queueUrl);
        }

        #endregion
    }
}
