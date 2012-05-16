using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.Model.GeneralModel
{
    public class MathHelpers
    {

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
      
        //converts ltr to gallons
        public static double LtrToGallons(double ltr)
        {
            double aGallon = 0.264172051;
        
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
        //converts a route time table entry to datetime
        public static DateTime ConvertEntryToDate(RouteTimeTableEntry entry)
        {
            int currentDay = (int)GameObject.GetInstance().GameTime.DayOfWeek;
            if (GameObject.GetInstance().GameTime.DayOfWeek > entry.Day)
                currentDay -= 7;
            
            int daysBetween = Math.Abs((int)entry.Day - currentDay);
            
            if (daysBetween == 0 && new TimeSpan(GameObject.GetInstance().GameTime.Hour,GameObject.GetInstance().GameTime.Minute,0) > new TimeSpan(entry.Time.Hours,entry.Time.Minutes,0))//.Add(new TimeSpan(1,0,0)))
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
     
    }
}
