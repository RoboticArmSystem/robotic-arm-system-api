using System;

namespace RoboticArmSystem.Core.Utils
{
    public class GetTimeTool
    {
        public static int GetNowTimestamp()
        {
            return (int)(DateTime.UtcNow.AddHours(8) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static DateTime GetNowDate()
        {
            return DateTime.UtcNow.AddHours(8);
        }

        public static string GetNowStrDate()
        {
            return DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static int TransDateToTimestamp(DateTime d)
        {
            return (int)(d - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static int TransStrDateToTimestamp(string d)
        {
            return (int)(Convert.ToDateTime(d) - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static DateTime TransTimestampToDate(int timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timestamp);
        }

        public static string TransTimestampToStrDate(int timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(timestamp).ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
