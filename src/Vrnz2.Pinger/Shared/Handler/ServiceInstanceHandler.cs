using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Vrnz2.Pinger.Crosscutting.Settings;

namespace Vrnz2.Pinger.Shared.Handler
{
    public class ServiceInstanceHandler
    {
        #region Variables

        private static ServiceInstanceHandler _instance;

        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static Random _random = new Random();

        #endregion

        #region Constructors

        private ServiceInstanceHandler()
            => Init();

        #endregion

        #region Attributes

        public static ServiceInstanceHandler Instance
        {
            get
            {
                _instance = _instance ?? new ServiceInstanceHandler();

                return _instance;
            }
        }

        public string GetServiceInstanceId { private set; get; }

        #endregion

        #region Methods

        private void Init()
        {
            var settings = Startup.GetService<IOptions<ServiceSettings>>().Value;

            GetServiceInstanceId = $"{settings.ServiceIdRoot}-{RandomString(5)}";
        }

        private static string RandomString(int length)
        => new string(Enumerable.Repeat(_chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());

        #endregion
    }
}
