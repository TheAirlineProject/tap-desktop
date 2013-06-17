using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using System.Globalization;
using TheAirline.Model.GeneralModel.WeatherModel;
using System.Security.Cryptography;

namespace TheAirline.Model.GeneralModel
{
    public class MathHelpers
    {
        private static Random rnd = new Random();
        //shuffles a list of items
        public static List<T> Shuffle<T>(List<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
        //returns the number of months between two dates
        public static int GetMonthsBetween(DateTime date1, DateTime date2)
        {
            return ((date2.Year - date1.Year) * 12) + date2.Month - date1.Month;

        }
        //returns a random date 
        public static DateTime GetRandomDate(DateTime minDate, DateTime maxDate)
        {
            int range = Math.Abs(((TimeSpan)(maxDate - minDate)).Days);

            int days = GetRandomInt(0,range);
          
            return new DateTime(minDate.AddDays(days).Year,minDate.AddDays(days).Month,minDate.AddDays(days).Day);


        }
        //returns a random number
        public static int GetRandomInt(int min, int max)
        {
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];

            rng.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);

            return new Random(result).Next(min, max);
        }
        //returns the age 
        public static int GetAge(DateTime date)
        {
            int years = GameObject.GetInstance().GameTime.Year - date.Year;

            date = date.AddYears(years);

            if (GameObject.GetInstance().GameTime.CompareTo(date) < 0) { years--; }

            return years;
        }
        //moves a object with coordinates in a direction for a specific distance in kilometers
        public static void MoveObject(Coordinates coordinates, Coordinates destination, double dist)
        {
            int rad = 6371;
            dist = dist / rad;  // convert dist to angular distance in radians
            double brng = MathHelpers.GetDirection(coordinates, destination);
            brng = MathHelpers.DegreeToRadian(brng);
            double lon1 = MathHelpers.DegreeToRadian(coordinates.Longitude.toDecimal());
            double lat1 = MathHelpers.DegreeToRadian(coordinates.Latitude.toDecimal());

            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(dist) +
                                  Math.Cos(lat1) * Math.Sin(dist) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(dist) * Math.Cos(lat1),
                                         Math.Cos(dist) - Math.Sin(lat1) * Math.Sin(lat2));
            lon2 = (lon2 + 3 * Math.PI) % (2 * Math.PI) - Math.PI;  // normalise to -180...+180

      
            if (Double.IsNaN(lat2))
                Console.WriteLine(destination.ToString());


            coordinates.Latitude = Coordinate.LatitudeToCoordinate(MathHelpers.RadianToDegree(lat2));
            coordinates.Longitude = Coordinate.LongitudeToCoordinate(MathHelpers.RadianToDegree(lon2));
        }
        //gets the angle between two coordinates
        public static double GetDirection(Coordinates coordinates1, Coordinates coordinates2)
        {


            var latitude1 = DegreeToRadian(coordinates1.Latitude.toDecimal());

            var latitude2 = DegreeToRadian(coordinates2.Latitude.toDecimal());


            var longitudeDifference = DegreeToRadian(coordinates2.Longitude.toDecimal() - coordinates1.Longitude.toDecimal());



            var y = Math.Sin(longitudeDifference) * Math.Cos(latitude2);

            var x = Math.Cos(latitude1) * Math.Sin(latitude2) -

                     Math.Sin(latitude1) * Math.Cos(latitude2) * Math.Cos(longitudeDifference);



            return (RadianToDegree(Math.Atan2(y, x)) + 360) % 360;

        }
        //retuns the wind direction from a direction
        public static Weather.WindDirection GetWindDirectionFromDirection(double direction)
        {
            if (direction < 45)
                return Weather.WindDirection.E;
            if (direction >= 45 && direction < 90)
                return Weather.WindDirection.NE;
            if (direction >= 90 && direction < 135)
                return Weather.WindDirection.N;
            if (direction >= 135 && direction < 180)
                return Weather.WindDirection.NW;
            if (direction >= 180 && direction < 225)
                return Weather.WindDirection.W;
            if (direction >= 225 && direction < 270)
                return Weather.WindDirection.SW;
            if (direction >= 270 && direction < 315)
                return Weather.WindDirection.S;
            if (direction >= 315 && direction < 360)
                return Weather.WindDirection.SE;

            return Weather.WindDirection.E;
        }
        //gets the distance between two airports in kilometers
        public static double GetDistance(Airport airport1, Airport airport2)
        {
            if (airport1.Statics == null)
                airport1.Statics = new AirportStatics(airport1);

            if (airport2.Statics == null)
                airport2.Statics = new AirportStatics(airport2);

            if (airport1.Statics.getDistance(airport2) == 0 && airport2.Statics.getDistance(airport1) == 0)
                return GetDistance(airport1.Profile.Coordinates, airport2.Profile.Coordinates);
            else
                return Math.Max(airport1.Statics.getDistance(airport2), airport2.Statics.getDistance(airport1));
          
        }
        //gets the distance in kilometers between two coordinates
        public static double GetDistance(Coordinates coordinates1, Coordinates coordinates2)
        {
           
            long circumference = 40074;

            double lat1 = coordinates1.Latitude.toDecimal();
            double lat2 = coordinates2.Latitude.toDecimal();
            double lon1 = coordinates1.Longitude.toDecimal();
            double lon2 = coordinates2.Longitude.toDecimal();

            double distLat = Math.Abs(lat1 - lat2);
            double distLon = Math.Abs(lon1 - lon2);
            double polar1 = 90 - lat1;
            double polar2 = 90 - lat2;

            double b = DegreeToRadian(polar1);
            double c = DegreeToRadian(polar2);
            double A = DegreeToRadian(distLon);

            double dist = (Math.Cos(b) * Math.Cos(c)) + (Math.Sin(b) * Math.Sin(c) * Math.Cos(A));
            dist = RadianToDegree(Math.Acos(dist));


            return circumference * dist / 360;
        }
        //returns the coordinates for a route in a distance of a specific lenghth
        public static Coordinates GetRoutePoint(Coordinates c1, Coordinates c2, double distance)
        {
           
            var tc = DegreeToRadian(GetDirection(c1, c2));
            const double radiusEarthKilometres = 6371.01;
            var distRatio = distance / radiusEarthKilometres;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = DegreeToRadian(c1.Latitude.toDecimal());
            var startLonRad = DegreeToRadian(c1.Longitude.toDecimal());

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(tc)));

            var endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(tc) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));

            return new Coordinates(Coordinate.LatitudeToCoordinate(RadianToDegree(endLatRads)), Coordinate.LongitudeToCoordinate(RadianToDegree(endLonRads)));

        }
        //returns the flight time between two coordinates with a given speed
        public static TimeSpan GetFlightTime(Coordinates coordinate1, Coordinates coordinate2, double speed)
        {

            if (coordinate1.CompareTo(coordinate2) == 0)
                return new TimeSpan();

            double dist = MathHelpers.GetDistance(coordinate1, coordinate2);

            double dtime = dist / speed;

            int hours = Convert.ToInt16(Math.Floor(dtime));

            double dMinutes = (dtime - hours) * 60;

            int minutes = Convert.ToInt16(Math.Floor(dMinutes));

            return new TimeSpan(hours, minutes, 0);
        }
        //returns the flight time for a given airliner type between two coordinates
        public static TimeSpan GetFlightTime(Coordinates coordinate1, Coordinates coordinate2, AirlinerType type)
        {
            double dist = MathHelpers.GetDistance(coordinate1, coordinate2);

            if (dist == 0)
                return new TimeSpan(0, 0, 0);

            double speed = type.CruisingSpeed;

        
            double dtime = dist / speed;

            int hours = Convert.ToInt16(Math.Floor(dtime));

            double dMinutes = (dtime - hours) * 60;

            int minutes = Convert.ToInt16(Math.Floor(dMinutes));

            return new TimeSpan(hours, minutes, 0);
        }
        public static TimeSpan GetFlightTime(Airport airport1, Airport airport2,AirlinerType type)
        {
            return GetFlightTime(airport1.Profile.Coordinates, airport2.Profile.Coordinates, type);
        }
        //converts gallons to ltr
        public static double GallonsToLtr(double gallons)
        {
            double aGallon = 0.264172051;
            
            return gallons * aGallon;
        }
        //converts ltr to gallons
        public static double LtrToGallons(double ltr)
        {
            double aGallon = 3.785411784;
            
            return ltr * aGallon;
        }
        //converts l/km to mpg
        public static double LKMToMPG(double kml)
        {
            double aMPG = 2.35;

            return kml * aMPG;
        }
        //converts km to miles
        public static double KMToMiles(double km)
        {
            double aMile = 1.609344;
            return km / aMile;
        }
        //converts miles to km
        public static double MilesToKM(double miles)
        {
            double aMile = 1.609344;
            return miles * aMile;
        }
        //converts meter to feet
        public static double MeterToFeet(double meter)
        {
            double aFeet = 3.2808399;
            return meter * aFeet;
        }
        //converts celsius to fahrenheit
        public static double CelsiusToFahrenheit(double celsius)
        {
            return celsius * 1.8 + 32;
        }
        //converts a radian to angle
        public static double RadianToDegree(double radian)
        {
            return radian * (180.0 / Math.PI);
        }
        //converts a angle to radian
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        //calculates an age from two dates
        public static int CalculateAge(DateTime startDate, DateTime endDate)
        {
            // cache the current time
            // get the difference in years
            int years = endDate.Year - startDate.Year;
            // subtract another year if we're before the
            // birth day in the current year
            if (endDate.Month < startDate.Month || (endDate.Month == startDate.Month && endDate.Day < startDate.Day))
                --years;

            return years;
        }
        //checks if it is a new month
        public static Boolean IsNewMonth(DateTime date)
        {
            return date.Day == 1 && IsNewDay(date);
        }
        //checks if it is a new year
        public static Boolean IsNewYear(DateTime date)
        {
            return date.Month == 1 && date.Day == 1 && IsNewDay(date);
        }
        //checks if it is a new day
        public static Boolean IsNewDay(DateTime date)
        {
            return date.TimeOfDay.Equals(new TimeSpan(0, 0, 0));
        }
        //returns a random double
        public static double GetRandomDoubleNumber(double minimum, double maximum)
        {
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] buffer = new byte[4];

            rng.GetBytes(buffer);
            int result = BitConverter.ToInt32(buffer, 0);

             return new Random(result).NextDouble() * (Math.Max(minimum,maximum) - Math.Min(maximum,minimum)) + Math.Min(maximum,minimum);
        }
        //converts a route time table entry to datetime
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
            int currentDay = (int)GameObject.GetInstance().GameTime.DayOfWeek;

            int entryDay = (int)entry.Day + entry.Time.Days; 

            if (currentDay > entryDay)
                currentDay -= 7;

            int daysBetween = Math.Abs(entryDay - currentDay);

            if (daysBetween == 0 && new TimeSpan(GameObject.GetInstance().GameTime.Hour, GameObject.GetInstance().GameTime.Minute, 0) > new TimeSpan(entry.Time.Hours, entry.Time.Minutes, 0).Add(new TimeSpan(0,maxMinutes,0)))
            {
                daysBetween = 7;
            }

            DateTime flightTime = new DateTime(GameObject.GetInstance().GameTime.Year, GameObject.GetInstance().GameTime.Month, GameObject.GetInstance().GameTime.Day, entry.Time.Hours, entry.Time.Minutes, entry.Time.Seconds).AddDays(daysBetween);


            return flightTime;
        }

        //gets the local time for a time in a time zone
        public static DateTime ConvertDateTimeToLoalTime(DateTime time, GameTimeZone timeZone)
        {
            TimeSpan delta = GameObject.GetInstance().TimeZone.UTCOffset.Subtract(timeZone.UTCOffset);

            return time.Subtract(delta);
        }
        public static TimeSpan ConvertTimeSpanToLocalTime(TimeSpan time, GameTimeZone timeZone)
        {
            TimeSpan delta = GameObject.GetInstance().TimeZone.UTCOffset.Subtract(timeZone.UTCOffset);

            return time.Subtract(delta);
        }
        //returns the monthly payment of a specific amount with a rate and length
        public static double GetMonthlyPayment(double amount, double rate, int length)
        {
             double pRate = 1+ rate / 100;
         
            return amount * pRate / length;
        }
        //returns the nth day for a given month and year
        public static DateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek day, int number)
        {
            DateTime tDate = new DateTime(year, month, 1);

            int diffDays = day - tDate.DayOfWeek;

            if (diffDays < 0)
                diffDays = 7 + diffDays;

            tDate = tDate.AddDays(diffDays).AddDays(7 * (number - 1));

            return tDate;


        }
        //returns the first date for a given week
        public static DateTime GetFirstDateOfWeek(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);

        }
    
     
    }
}
