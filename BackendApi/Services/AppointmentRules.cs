using System;

namespace BackendApi.Services
{
    public static class AppointmentRules
    {
        // Clinic hours in Los Angeles
        private const int OpenHour = 8;   // 08:00
        private const int CloseHour = 17; // 17:00

        private static TimeZoneInfo GetLaTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"); }   // Linux/Render
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");     // Windows dev box
            }
        }

        /// <summary>
        /// Validate weekday + business hours in Los Angeles.
        /// If your input is UTC, set isUtc=true so we convert correctly.
        /// </summary>
        public static (bool ok, string? error) ValidateBusinessWindow(
            DateTime startsAt,
            int durationMinutes,
            bool isUtc = false)
        {
            if (durationMinutes <= 0 || durationMinutes > 12 * 60)
                return (false, "Duration must be between 1 and 720 minutes.");

            var tz = GetLaTz();

            // Normalize to LA local time for validation
            DateTime laStartLocal;
            if (isUtc)
                laStartLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(startsAt, DateTimeKind.Utc), tz);
            else
                laStartLocal = DateTime.SpecifyKind(startsAt, DateTimeKind.Unspecified); // treat as LA local

            var laEndLocal = laStartLocal.AddMinutes(durationMinutes);

            // Weekend check
            if (laStartLocal.DayOfWeek == DayOfWeek.Saturday || laStartLocal.DayOfWeek == DayOfWeek.Sunday)
                return (false, "Clinic is closed on weekends.");

            // Hours check: start >= 08:00, end <= 17:00
            var open = new TimeSpan(OpenHour, 0, 0);
            var close = new TimeSpan(CloseHour, 0, 0);

            if (laStartLocal.TimeOfDay < open || laEndLocal.TimeOfDay > close)
                return (false, "Time is outside clinic hours (08:00–17:00).");

            return (true, null);
        }
    }
}
