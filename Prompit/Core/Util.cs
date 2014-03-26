using System;

namespace XPFriend.Prompit.Core
{
    internal class Util
    {
        internal static string Format(TimeSpan timeSpan)
        {
            string format = (timeSpan.Hours > 0) ? @"hh\:mm\:ss" : @"mm\:ss";
            return timeSpan.ToString(format);
        }
    }
}
