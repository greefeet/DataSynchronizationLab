using System;

namespace DataSynchronizationLab.Model
{
    public class ServiceKeyTime
    {
        public static string Get()
        {
            DateTime LimitDate = new DateTime(2999, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return LimitDate.Subtract(DateTime.UtcNow).TotalMilliseconds.ToString("00000000000000.00").Replace(".", "");
        }
    }
}
