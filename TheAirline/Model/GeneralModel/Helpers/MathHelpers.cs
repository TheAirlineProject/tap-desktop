using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;
using System.Windows;
using System.Windows.Threading;
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
        public static double ToDegree(double val) { return val * 180 / Math.PI; } 
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
            //left for readability
            int days = rnd.Next(0,Math.Abs(((TimeSpan)(maxDate - minDate)).Days));

            return new DateTime(minDate.AddDays(days).Year, minDate.AddDays(days).Month, minDate.AddDays(days).Day);


        }
        //returns a random number
        public static int GetRandomInt(int min, int max)
        {
            return rnd.Next(min, max);
        }
        //returns the age 
        public static int GetAge(DateTime date)
        {
            int years = GameObject.GetInstance().GameTime.Year - date.Year;

            date = date.AddYears(years);

            if (GameObject.GetInstance().GameTime.CompareTo(date) < 0) { years--; }

            return years;
        }
        //moves an airliner with a specific speed
        public static void MoveObject(FleetAirliner airliner, double speed)
        {
            airliner.CurrentFlight.DistanceToDestination -= speed;
        }
        //moves a object with coordinates in a direction for a specific distance in kilometers
        public static void MoveObject(GeoCoordinate coordinates, GeoCoordinate destination, double dist, double speed)
        {
            
            
            //Get an Degree of the current flight plan
            var dLat = coordinates.Latitude - destination.Latitude;
            var dLon = destination.Longitude - coordinates.Longitude;
            var dPhi = Math.Log(Math.Tan(destination.Latitude / 2 + Math.PI / 4) / Math.Tan(coordinates.Latitude / 2 + Math.PI / 4));
            var q = (Math.Abs(dLat) > 0) ? dLat / dPhi : Math.Cos(coordinates.Latitude);
            
            if (Math.Abs(dLon) > Math.PI)
            {
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            }
            var brng = ToDegree(Math.Atan2(dLon, dPhi));

            //Now calculate the 
            var now = DateTime.Now;
            GeoCoordinate oldPosition = coordinates;
            double newSpeed = speed;
            TimeSpan timeperturn = TimeSpan.FromTicks(36000000000);
            TimeSpan timeParsed = timeperturn;
            //Doing *2 atm because it was heading the wrong way.
            double newCourse = brng*2;
			while (newCourse < 0) newCourse += 360;
			while (newCourse >= 360) newCourse -= 360;
            double distanceTravelled = (newSpeed + newSpeed) * .5 * timeParsed.TotalSeconds;
            double accuracy = Math.Min(500, Math.Max(20, oldPosition.HorizontalAccuracy + (rnd.NextDouble() * 100 - 50)));       
            var pos = GetPointFromHeadingGeodesic(new Point(oldPosition.Longitude, oldPosition.Latitude), distanceTravelled, newCourse - 180);

            var newPosition = new GeoPosition<GeoCoordinate>(
                   new DateTimeOffset(now), new GeoCoordinate()
                   {
                       Latitude = pos.Y,
                       Longitude = pos.X,
                       Altitude = 10000,
                       Speed = newSpeed,
                       Course = newCourse,
                       HorizontalAccuracy = accuracy,
                       VerticalAccuracy = rnd.NextDouble() * 300,
                   });
            coordinates = new GeoCoordinate(newPosition.Location.Latitude, newPosition.Location.Longitude);
            //coordinates.Latitude = newPosition.Location.Latitude;
            //coordinates.Longitude = newPosition.Location.Longitude;
        }
        //added newly for route simulation
        private static Point GetPointFromHeadingGeodesic(Point start, double distance, double heading)
        {
            double brng = heading / 180 * Math.PI;
            double lon1 = start.X / 180 * Math.PI;
            double lat1 = start.Y / 180 * Math.PI;
            double dR = distance / 6378137; //Angular distance in radians
            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(dR) + Math.Cos(lat1) * Math.Sin(dR) * Math.Cos(brng));
            double lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(dR) * Math.Cos(lat1), Math.Cos(dR) - Math.Sin(lat1) * Math.Sin(lat2));
            double lon = lon2 / Math.PI * 180;
            double lat = lat2 / Math.PI * 180;
            while (lon < -180) lon += 360;
            while (lat < -90) lat += 180;
            while (lon > 180) lon -= 360;
            while (lat > 90) lat -= 180;
            return new Point(lon, lat);
        }

        //gets the angle between two coordinates
        public static double GetDirection(GeoCoordinate coordinates1, GeoCoordinate coordinates2)
        {
             var latitude1 = DegreeToRadian(coordinates1.Latitude);

            var latitude2 = DegreeToRadian(coordinates2.Latitude);

            var longitudeDifference = MathHelpers.DegreeToRadian(coordinates2.Longitude - coordinates1.Longitude);



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
                return airport1.Profile.Coordinates.GetDistanceTo(airport2.Profile.Coordinates) / 1000;
            else
                return Math.Max(airport1.Statics.getDistance(airport2), airport2.Statics.getDistance(airport1));
          
        }

        public static double DMStoDeg(int degrees, int minutes, int seconds)
        {
            return Convert.ToDouble(degrees) + (Convert.ToDouble(minutes) / 60) + (Convert.ToDouble(seconds) / 3600);
        }

        //gets the distance in kilometers between two coordinates
        public static double GetDistance(GeoCoordinate c1, GeoCoordinate c2)
        {
            return c1.GetDistanceTo(c2) / 1000;
        }
        //returns the coordinates for a route in a distance of a specific lenghth
        public static GeoCoordinate GetRoutePoint(GeoCoordinate c1, GeoCoordinate c2, double distance)
        {
           
            var tc = DegreeToRadian(GetDirection(c1, c2));
            const double radiusEarthKilometres = 6371.01;
            var distRatio = distance / radiusEarthKilometres;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);

            var startLatRad = DegreeToRadian(c1.Latitude);
            var startLonRad = DegreeToRadian(c1.Longitude);

            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);

            double endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(tc)));

            double endLonRads = startLonRad
                + Math.Atan2(
                    Math.Sin(tc) * distRatioSine * startLatCos,
                    distRatioCosine - startLatSin * Math.Sin(endLatRads));

            return new GeoCoordinate(RadianToDegree(endLatRads), RadianToDegree(endLonRads));

        }
        //returns the flight time between two coordinates with a given speed
        public static TimeSpan GetFlightTime(GeoCoordinate coordinate1, GeoCoordinate coordinate2, double speed)
        {

            if (coordinate1.Equals(coordinate2))
                return new TimeSpan();

            double dist = GetDistance(coordinate1,coordinate2);

            double dtime = dist / speed;

            int hours = Convert.ToInt16(Math.Floor(dtime));

            double dMinutes = (dtime - hours) * 60;

            int minutes = Convert.ToInt16(Math.Floor(dMinutes));

            return new TimeSpan(hours, minutes, 0);
        }
        //returns the flight time for a given airliner type between two coordinates
        public static TimeSpan GetFlightTime(GeoCoordinate coordinate1, GeoCoordinate coordinate2, AirlinerType type)
        {
            double dist = GetDistance(coordinate1,coordinate2);

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
           
            return rnd.NextDouble() * (maximum - minimum) + minimum;
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
