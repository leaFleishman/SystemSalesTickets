namespace SystemSalesTickets.API.Middleware
{
    public static class SunsetCalculator
    {
        // ברירת מחדל: תל אביב. אפשר לשנות לפי המיקום הרלוונטי
        private const double DefaultLatitude = 32.0853;
        private const double DefaultLongitude = 34.7818;

        public static DateTime GetSunset(DateTime date, double latitude = DefaultLatitude, double longitude = DefaultLongitude)
        {
            // חישוב שקיעה לפי אלגוריתם NOAA (מקורב, דיוק של כמה דקות)
            int dayOfYear = date.DayOfYear;

            double lngHour = longitude / 15.0;
            double t = dayOfYear + ((18 - lngHour) / 24.0);

            double M = (0.9856 * t) - 3.289;
            double L = M + (1.916 * Math.Sin(ToRad(M))) + (0.020 * Math.Sin(ToRad(2 * M))) + 282.634;
            L = NormalizeDegrees(L);

            double RA = ToDeg(Math.Atan(0.91764 * Math.Tan(ToRad(L))));
            RA = NormalizeDegrees(RA);

            double Lquadrant = Math.Floor(L / 90.0) * 90.0;
            double RAquadrant = Math.Floor(RA / 90.0) * 90.0;
            RA = RA + (Lquadrant - RAquadrant);
            RA = RA / 15.0;

            double sinDec = 0.39782 * Math.Sin(ToRad(L));
            double cosDec = Math.Cos(Math.Asin(sinDec));

            double zenith = 90.833; // זווית רשמית לשקיעה (כולל שבירת אור)
            double cosH = (Math.Cos(ToRad(zenith)) - (sinDec * Math.Sin(ToRad(latitude)))) / (cosDec * Math.Cos(ToRad(latitude)));

            if (cosH > 1)
                throw new InvalidOperationException("השמש לא שוקעת בתאריך/מיקום זה");
            if (cosH < -1)
                throw new InvalidOperationException("השמש לא זורחת בתאריך/מיקום זה");

            double H = ToDeg(Math.Acos(cosH));
            H = H / 15.0;

            double T = H + RA - (0.06571 * t) - 6.622;

            double UT = T - lngHour;
            UT = NormalizeHours(UT);

            // UT הוא בזמן UTC - נמיר לזמן מקומי (ישראל)
            var utcDateTime = date.Date.AddHours(UT);
            var israelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, israelTimeZone);
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;
        private static double ToDeg(double rad) => rad * 180.0 / Math.PI;

        private static double NormalizeDegrees(double deg)
        {
            if (deg < 0) deg += 360;
            if (deg >= 360) deg -= 360;
            return deg;
        }

        private static double NormalizeHours(double hours)
        {
            if (hours < 0) hours += 24;
            if (hours >= 24) hours -= 24;
            return hours;
        }
    }

   

namespace SystemSalesTickets.API.Middleware
    {
        public class CheckShabatMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<CheckShabatMiddleware> _logger;

            private const int MinutesBeforeSunsetFriday = 20;
            private const int MinutesAfterSunsetSaturday = 40;

            public CheckShabatMiddleware(RequestDelegate next, ILogger<CheckShabatMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                var israelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, israelTimeZone);

                var (isShabbat, entryTime, exitTime) = IsWithinShabbat(now);

                if (isShabbat)
                {
                    _logger.LogWarning(
                        "Access blocked - Shabbat period ({Entry} - {Exit}), request at {Now}",
                        entryTime, exitTime, now);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"message\":\"המערכת סגורה בשבת. אנא נסו שוב לאחר צאת השבת.\"}");
                    return;
                }

                await _next(context);
            }

            private (bool isShabbat, DateTime entry, DateTime exit) IsWithinShabbat(DateTime now)
            {
                // מוצאים את יום שישי ושבת של אותו שבוע (הקרוב, לא בעבר)
                DateTime friday = GetRelevantFriday(now);
                DateTime saturday = friday.AddDays(1);

                DateTime fridaySunset = SunsetCalculator.GetSunset(friday);
                DateTime saturdaySunset = SunsetCalculator.GetSunset(saturday);

                DateTime shabbatEntry = fridaySunset.AddMinutes(-MinutesBeforeSunsetFriday);
                DateTime shabbatExit = saturdaySunset.AddMinutes(MinutesAfterSunsetSaturday);

                bool isWithin = now >= shabbatEntry && now <= shabbatExit;

                return (isWithin, shabbatEntry, shabbatExit);
            }

            private static DateTime GetRelevantFriday(DateTime now)
            {
                // אם היום שישי או שבת - השישי הרלוונטי הוא השבוע הזה
                // אחרת - השישי הבא
                int daysUntilFriday = ((int)DayOfWeek.Friday - (int)now.DayOfWeek + 7) % 7;

                if (now.DayOfWeek == DayOfWeek.Saturday)
                    return now.Date.AddDays(-1); // אתמול היה שישי

                return now.Date.AddDays(daysUntilFriday);
            }
        }

        public static class CheckShabatMiddlewareExtensions
        {
            public static IApplicationBuilder UseCheckShabatMiddleware(this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<CheckShabatMiddleware>();
            }
        }
    }



}