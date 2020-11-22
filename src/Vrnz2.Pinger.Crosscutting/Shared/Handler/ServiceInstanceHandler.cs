using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Vrnz2.Pinger.Crosscutting.Settings;

namespace Vrnz2.Pinger.Crosscutting.Shared.Handler
{
    public class ServiceInstanceHandler
    {
        #region Variables

        private static ServiceInstanceHandler _instance;

        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static Random _random = new Random();

        #endregion

        #region Constructors

        private ServiceInstanceHandler(ServiceSettings serviceSettings)
            => Init(serviceSettings);

        #endregion

        #region Attributes

        public static ServiceInstanceHandler Instance(ServiceSettings serviceSettings)
        {
            _instance = _instance ?? new ServiceInstanceHandler(serviceSettings);

            return _instance;
        }

        public string GetServiceInstanceId { private set; get; }

        #endregion

        #region Methods

        private void Init(ServiceSettings serviceSettings)
            => GetServiceInstanceId = $"{serviceSettings.ServiceIdRoot}-{RandomString(5)}";

        private static string RandomString(int length)
        => new string(Enumerable.Repeat(_chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());

        #endregion
    }
}
