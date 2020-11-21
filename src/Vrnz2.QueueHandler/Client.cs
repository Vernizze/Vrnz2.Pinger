using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Vrnz2.QueueHandler
{
    public class Client
            : IDisposable
    {
        #region Constructors

        internal Client(string accessKey, string secretKey, string region)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            AwsClient = new AmazonSQSClient(credentials, RegionEndpoint.GetBySystemName(region));
        }

        internal Client(string region)
        {
            AwsClient = new AmazonSQSClient(RegionEndpoint.GetBySystemName(region));
        }

        #endregion

        #region Attributes        

        internal AmazonSQSClient AwsClient { get; private set; } = null;

        #endregion

        #region Methods

        public void Dispose()
        {
            AwsClient.Dispose();
        }

        public async Task CreateQueue(string queueName)
        {
            var response = await AwsClient.CreateQueueAsync(queueName);

            if (!HttpStatusCode.OK.Equals(response.HttpStatusCode))
                throw new Exception($"Creating Queue Error! StatusCode: {response.HttpStatusCode}");
        }

        public async Task DeleteQueue(string queueName)
        {
            var response = await AwsClient.DeleteQueueAsync(queueName);

            if (!HttpStatusCode.OK.Equals(response.HttpStatusCode))
                throw new Exception($"Deleting Queue Error! StatusCode: {response.HttpStatusCode}");
        }

        #endregion
    }
}
