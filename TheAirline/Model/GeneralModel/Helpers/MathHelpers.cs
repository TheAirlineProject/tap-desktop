using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Security.Cryptography;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.WeatherModel;
using TheAirline.Model.RouteModel;

namespace TheAirline.Model.GeneralModel.Helpers
{
    public class MathHelpers
    {
        #region Static Fields

        private static readonly Randomizer Rnd = new Randomizer();

        #endregion

        private const double Mile = 1.609344;

        #region Public Methods and Operators

        public static int CalculateAge(DateTime startDate, DateTime endDate)
        {
            // cache the current time
            // get the difference in years
            int years = endDate.Year - startDate.Year;
            // subtract another year if we're before the
            // birth day in the current year
            if (endDate.Month < startDate.Month || (endDate.Month == startDate.Month && endDate.Day < startDate.Day))
            {
                --years;
            }

            return years;
        }

        public static double CelsiusToFahrenheit(double celsius)
        {
            return celsius*1.8 + 32;
        }

        public static DateTime ConvertDateTimeToLoalTime(DateTime time, GameTimeZone timeZone)
        {
            TimeSpan delta = GameObject.GetInstance().TimeZone.UTCOffset.Subtract(timeZone.UTCOffset);

            return time.Subtract(delta);
        }

        public static DateTime ConvertEntryToDate(RouteTimeTableEntry entry)
        {
            return ConvertEntryToDate(entry, 0);
            /*
            int currentDay = (int)GameObject.GetInstance().GameTime.DayOfWeek;
            if (GameObject.GetInstance().GameTime.DayOfWeek > entry.Day) 
                currentDay -= 7;
            
            int daysBetween = Math.Abs((int)entry.Day - currentDay);
            
            if (daysBetween == 0 && new TimeSpan(GameObject.GetInstance().GameTime.Hour,GameObject.GetInstance().GameTime.Minute,0) > new TimeSpan(entry.Time.Hours,entry.Time.Minutes,0))
            {
                daysBetween = 7;
            }

            DateTime flightTime = new DateTime(GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, GameObject.GetInstance().GameTime.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds).AddDays(daysBetween);

            
            return flightTime;
             * */
        }

        //converts a route time table entry to datetime with a max minutes before moving a week ahead
        public static DateTime ConvertEntryToDate(RouteTimeTableEntry entry, int maxMinutes)
        {
            var currentDay = (int) GameObject.GetInstance().GameTime.DayOfWeek;

            int entryDay = (int) entry.Day + entry.Time.Days;

            if (currentDay > entryDay)
            {
                currentDay -= 7;
            }

            int daysBetween = Math.Abs(entryDay - currentDay);

            if (daysBetween == 0
                && new TimeSpan(GameObject.GetInstance().GameTime.Hour, GameObject.GetInstance().GameTime.Minute, 0)
                > new TimeSpan(entry.Time.Hours, entry.Time.Minutes, 0).Add(new TimeSpan(0, maxMinutes, 0)))
            {
                daysBetween = 7;
            }

            DateTime flightTime =
                new DateTime(
                    GameObject.GetInstance().GameTime.Year,
                    GameObject.GetInstance().GameTime.Month,
                    GameObject.GetInstance().GameTime.Day,
                    entry.Time.Hours,
                    entry.Time.Minutes,
                    entry.Time.Seconds).AddDays(daysBetween);

            return flightTime;
        }

        public static TimeSpan ConvertTimeSpanToLocalTime(TimeSpan time, GameTimeZone timeZone)
        {
            TimeSpan delta = GameObject.GetInstance().TimeZone.UTCOffset.Subtract(timeZone.UTCOffset);

            return time.Subtract(delta);
        }

        public static double DMStoDeg(int degrees, int minutes, int seconds)
        {
            double d = Convert.ToDouble(Math.Abs(degrees));
            double m = Convert.ToDouble(minutes)/60;
            double s = Convert.ToDouble(seconds)/3600;

            return degrees < 0 ? -(d + m + s) : d + m + s;

            //return Convert.ToDouble(degrees) + (Convert.ToDouble(minutes) / 60) + (Convert.ToDouble(seconds) / 3600);
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI*angle/180.0;
        }

        public static double GallonsToLtr(double gallons)
        {
            const double aGallon = 0.264172051;

            return gallons*aGallon;
        }

        //returns the age 
        public static int GetAge(DateTime date)
        {
            int years = GameObject.GetInstance().GameTime.Year - date.Year;

            date = date.AddYears(years);

            if (GameObject.GetInstance().GameTime.CompareTo(date) < 0)
            {
                years--;
            }

            return years;
        }

        //returns the age in months
        public static int GetAgeMonths(DateTime date)
        {
            return ((GameObject.GetInstance().GameTime.Year - date.Year)*12) + GameObject.GetInstance().GameTime.Month - date.Month;
        }

        //moves a object by substracting the speed from the distance

        //gets the angle between two coordinates
        public static double GetDirection(GeoCoordinate coordinates1, GeoCoordinate coordinates2)
        {
            double latitude1 = DegreeToRadian(coordinates1.Latitude);

            double latitude2 = DegreeToRadian(coordinates2.Latitude);

            double longitudeDifference = DegreeToRadian(coordinates2.Longitude - coordinates1.Longitude);

            double y = Math.Sin(longitudeDifference)*Math.Cos(latitude2);

            double x = Math.Cos(latitude1)*Math.Sin(latitude2)
                       - Math.Sin(latitude1)*Math.Cos(latitude2)*Math.Cos(longitudeDifference);

            return (RadianToDegree(Math.Atan2(y, x)) + 360)%360;
        }

        //retuns the wind direction from a direction

        //gets the distance between two airports in kilometers
        public static double GetDistance(Airport airport1, Airport airport2)
        {
            if (airport1.Statics == null)
            {
                airport1.Statics = new AirportStatics(airport1);
            }

            if (airport2.Statics == null)
            {
                airport2.Statics = new AirportStatics(airport2);
            }

            if (airport1.Statics.GetDistance(airport2) == 0 && airport2.Statics.GetDistance(airport1) == 0)
            {
                return
                    airport1.Profile.Coordinates.ConvertToGeoCoordinate()
                            .GetDistanceTo(airport2.Profile.Coordinates.ConvertToGeoCoordinate())/1000;
            }
            return Math.Max(airport1.Statics.GetDistance(airport2), airport2.Statics.GetDistance(airport1));
        }

        //gets the distance in kilometers between two coordinates
        public static double GetDistance(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.GetDistanceTo(c2)/1000;
        }

        public static DateTime GetFirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            Calendar cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            int weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime result = firstThursday.AddDays(weekNum*7);
            return result.AddDays(-3);
        }

        //returns the coordinates for a route in a distance of a specific lenghth

        //returns the flight time between two coordinates with a given speed
        public static TimeSpan GetFlightTime(GeoCoordinate coordinate1, GeoCoordinate coordinate2, double speed)
        {
            if (coordinate1.Equals(coordinate2))
            {
                return new TimeSpan();
            }

            double dist = GetDistance(coordinate1, coordinate2);

            double dtime = dist/speed;

            int hours = Convert.ToInt16(Math.Floor(dtime));

            double dMinutes = (dtime - hours)*60;

            int minutes = Convert.ToInt16(Math.Floor(dMinutes));

            //2.5 minutes for takeoff and 2.5 minutes for landing
            minutes += 5;

            return new TimeSpan(hours, minutes, 0);
        }

        //returns the flight time for a given airliner type between two coordinates
        public static TimeSpan GetFlightTime(GeoCoordinate coordinate1, GeoCoordinate coordinate2, AirlinerType type)
        {
            double dist = GetDistance(coordinate1, coordinate2);

            if (dist == 0)
            {
                return new TimeSpan(0, 0, 0);
            }

            double speed = type.CruisingSpeed;

            double dtime = dist/speed;

            int hours = Convert.ToInt16(Math.Floor(dtime));

            double dMinutes = (dtime - hours)*60;

            int minutes = Convert.ToInt16(Math.Floor(dMinutes));

            //2.5 minutes for takeoff and 2.5 minutes for landing
            minutes += 5;

            return new TimeSpan(hours, minutes, 0);
        }

        public static TimeSpan GetFlightTime(Airport airport1, Airport airport2, AirlinerType type)
        {
            return GetFlightTime(
                airport1.Profile.Coordinates.ConvertToGeoCoordinate(),
                airport2.Profile.Coordinates.ConvertToGeoCoordinate(),
                type);
        }

        public static double GetMonthlyPayment(double amount, double rate, int length)
        {
            double pRate = 1 + rate/100;

            return amount*pRate/length;
        }

        public static int GetMonthsBetween(DateTime date1, DateTime date2)
        {
            return ((date2.Year - date1.Year)*12) + date2.Month - date1.Month;
        }

        public static DateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek day, int number)
        {
            var tDate = new DateTime(year, month, 1);

            int diffDays = day - tDate.DayOfWeek;

            if (diffDays < 0)
            {
                diffDays = 7 + diffDays;
            }

            tDate = tDate.AddDays(diffDays).AddDays(7*(number - 1));

            return tDate;
        }

        public static DateTime GetRandomDate(DateTime minDate, DateTime maxDate)
        {
            TimeSpan range = maxDate - minDate;

            var randTimeSpan = new TimeSpan((long) (Rnd.NextDouble()*range.Ticks));

            return minDate + randTimeSpan;
        }

        public static double GetRandomDoubleNumber(double minimum, double maximum)
        {
            return Rnd.NextDouble()*(maximum - minimum) + minimum;
        }

        public static int GetRandomInt(int min, int max)
        {
            return Rnd.Next(min, max);
        }

        public static GeoCoordinate GetRoutePoint(GeoCoordinate c1, GeoCoordinate c2, double distance)
        {
            double tc = DegreeToRadian(GetDirection(c1, c2));
            const double radiusEarthKilometres = 6371.01;
            double distRatio = distance/radiusEarthKilometres;
            double distRatioSine = Math.Sin(distRatio);
            double distRatioCosine = Math.Cos(distRatio);

            double startLatRad = DegreeToRadian(c1.Latitude);
            double startLonRad = DegreeToRadian(c1.Longitude);

            double startLatCos = Math.Cos(startLatRad);
            double startLatSin = Math.Sin(startLatRad);

            double endLatRads = Math.Asin(
                (startLatSin*distRatioCosine) + (startLatCos*distRatioSine*Math.Cos(tc)));

            double endLonRads = startLonRad
                                + Math.Atan2(
                                    Math.Sin(tc)*distRatioSine*startLatCos,
                                    distRatioCosine - startLatSin*Math.Sin(endLatRads));

            double lat = RadianToDegree(endLatRads);
            double lon = RadianToDegree(endLonRads);

            while (lon < -180 || lon > 180)
            {
                if (lon < -180)
                {
                    lon = 180 + (lon + 180);
                }

                if (lon > 180)
                {
                    lon = -180 + (lon - 180);
                }
            }

            return new GeoCoordinate(lat, lon);
        }

        public static Weather.WindDirection GetWindDirectionFromDirection(double direction)
        {
            if (direction < 45)
            {
                return Weather.WindDirection.E;
            }
            if (direction >= 45 && direction < 90)
            {
                return Weather.WindDirection.NE;
            }
            if (direction >= 90 && direction < 135)
            {
                return Weather.WindDirection.N;
            }
            if (direction >= 135 && direction < 180)
            {
                return Weather.WindDirection.NW;
            }
            if (direction >= 180 && direction < 225)
            {
                return Weather.WindDirection.W;
            }
            if (direction >= 225 && direction < 270)
            {
                return Weather.WindDirection.SW;
            }
            if (direction >= 270 && direction < 315)
            {
                return Weather.WindDirection.S;
            }
            if (direction >= 315 && direction < 360)
            {
                return Weather.WindDirection.SE;
            }

            return Weather.WindDirection.E;
        }

        public static Boolean IsNewDay(DateTime date)
        {
            return date.TimeOfDay.Equals(new TimeSpan(0, 0, 0));
        }

        public static Boolean IsNewMonth(DateTime date)
        {
            return date.Day == 1 && IsNewDay(date);
        }

        //checks if it is a new year
        public static Boolean IsNewYear(DateTime date)
        {
            return date.Month == 1 && date.Day == 1 && IsNewDay(date);
        }

        //converts gallons to ltr

        //converts km to miles
        public static double KMToMiles(double km)
        {
            return km/Mile;
        }

        public static double MilesToKM(double miles)
        {
            return miles*Mile;
        }

        public static double LKMToMPG(double kml)
        {
            const double aMPG = 2.35;

            return kml*aMPG;
        }

        public static double LSeatKMToGSeatM(double lsk)
        {
            return lsk/LtrToGallons(1)/KMToMiles(1);
        }

        public static double LtrToGallons(double ltr)
        {
            const double aGallon = 0.264172051;

            return ltr/aGallon;
        }

        //converts kg to pound
        public static double KgToPound(double kg)
        {
            const double perpound = 0.45359237;

            return kg/perpound;
        }

        //converts miles to km

        //converts meter to feet
        public static double MeterToFeet(double meter)
        {
            const double aFeet = 3.2808399;
            return meter*aFeet;
        }

        public static double FeetToMeter(double feet)
        {
            const double aFeet = 3.2808399;
            return feet/aFeet;
        }


        public static void MoveObject(FleetAirliner airliner, double speed)
        {
            double distance = airliner.CurrentFlight.DistanceToDestination;
            double timepermove = Settings.GetInstance().MinutesPerTurn;

            //Making sure that if the game time is not an hour, the plane is not moving an hour forward.
            if (timepermove == 15)
            {
                speed = speed/4;
            }
            else if (timepermove == 30)
            {
                speed = speed/2;
            }

            double distanceToDestination = distance - speed;
            if (distanceToDestination < 0)
            {
                distanceToDestination = 0;
            }

            airliner.CurrentFlight.DistanceToDestination = distanceToDestination;
        }

        //converts celsius to fahrenheit

        //converts a radian to angle
        public static double RadianToDegree(double radian)
        {
            return radian*(180.0/Math.PI);
        }

        public static List<T> Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static double ToDegree(double val)
        {
            return val*180/Math.PI;
        }

        #endregion

        //converts a angle to radian
    }

    //a new randomizer class
    public class Randomizer : RandomNumberGenerator
    {
        #region Static Fields

        private static RandomNumberGenerator _r;

        #endregion

        #region Constructors and Destructors

        public Randomizer()
        {
            _r = Create();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        public override void GetBytes(byte[] buffer)
        {
            _r.GetBytes(buffer);
        }

        /// <summary>
        ///     Returns a random number within the specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. maxValue must be greater than or equal
        ///     to minValue.
        /// </param>
        public int Next(int minValue, int maxValue)
        {
            return (int) Math.Round(NextDouble()*(maxValue - minValue - 1)) + minValue;
        }

        /// <summary>
        ///     Returns a nonnegative random number.
        /// </summary>
        public int Next()
        {
            return Next(0, Int32.MaxValue);
        }

        /// <summary>
        ///     Returns a nonnegative random number less than the specified maximum
        /// </summary>
        /// <param name="maxValue">
        ///     The inclusive upper bound of the random number returned. maxValue must be greater than or equal
        ///     0
        /// </param>
        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        /// <summary>
        ///     Returns a random number between 0.0 and 1.0.
        /// </summary>
        public double NextDouble()
        {
            var b = new byte[4];
            _r.GetBytes(b);
            return (double) BitConverter.ToUInt32(b, 0)/UInt32.MaxValue;
        }

        #endregion
    }
}