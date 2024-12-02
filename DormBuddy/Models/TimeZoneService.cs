using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DormBuddy.Models {

    public class TimeZoneService
    {
        // Convert Unix time zones to IANA format
        public string GetUnixTimeZoneId(string windowsTimeZoneId)
        {
            var timeZoneMapping = new Dictionary<string, string>
            {
                { "Pacific Standard Time", "America/Los_Angeles" },
                { "Mountain Standard Time", "America/Denver" },
                { "Central Standard Time", "America/Chicago" },
                { "Eastern Standard Time", "America/New_York" },
                { "Atlantic Standard Time", "America/Halifax" },
                { "Greenland Standard Time", "America/Godthab" },
                { "Western European Time", "Europe/Lisbon" },
                { "Central European Time", "Europe/Berlin" },
                { "Eastern European Time", "Europe/Bucharest" },
                { "Moscow Standard Time", "Europe/Moscow" },
                { "Arabian Standard Time", "Asia/Riyadh" },
                { "India Standard Time", "Asia/Kolkata" },
                { "China Standard Time", "Asia/Shanghai" },
                { "Japan Standard Time", "Asia/Tokyo" },
                { "Australian Eastern Time", "Australia/Sydney" },
                { "New Zealand Standard Time", "Pacific/Auckland" }
            };

            return timeZoneMapping.ContainsKey(windowsTimeZoneId) ? timeZoneMapping[windowsTimeZoneId] : "UTC";
        }

        // Main method that converts time based on OS platform
        public DateTime ConvertToLocal(DateTime utcDateTime, string timeZoneId)
        {
            // Adjust for OS-specific time zone ID
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                timeZoneId = GetUnixTimeZoneId(timeZoneId); // Use Unix time zone ID if on a Unix-based system
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
        }

        public DateTime getCurrentTimeFromUTC(DateTime date, string userTimeZoneId) {
            DateTime time = date;

            DateTime local = ConvertToLocal(time, userTimeZoneId);

            return local;
        }
    }
}