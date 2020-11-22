using System;

namespace Vrnz2.Pinger.Crosscutting.Utils
{
    public static class DateTimeUtils
    {
        #region Constants

        public static readonly DateTime BASE_DATETIME_UNIX_TIMESTAMP = new DateTime(1970, 1, 1);

        #endregion

        #region Methods

        public static double UnixTimestamp()
            => (DateTime.UtcNow.Subtract(BASE_DATETIME_UNIX_TIMESTAMP)).TotalSeconds;

        public static DateTime UnixTimestamp(double seconds)
            => new DateTime(BASE_DATETIME_UNIX_TIMESTAMP.Year, BASE_DATETIME_UNIX_TIMESTAMP.Month, BASE_DATETIME_UNIX_TIMESTAMP.Day).AddSeconds(seconds);

        #endregion
    }
}
