using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.Model.GeneralModel
{
    //class for some general helpers
    public class GeneralHelpers
    {
        public enum GameSpeedValue { Slowest = 700, Slow = 550, Normal = 400, Fast = 250, Fastest = 100 }
        public static string BigMapXaml;
        // chs, 2011-17-11 changed for showing departures and arrivals from one airport to another
        //returns the list of arrivals or departures from one airport to another
        public static List<RouteTimeTableEntry> GetAirportFlights(Airport fAirport, Airport tAirport, Boolean arrivals)
        {
            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in fAirport.Terminals.getRoutes())
            {
                if (route.Airliner != null && (route.Destination1 == tAirport || route.Destination2 == tAirport))
                {
                    RouteTimeTableEntry entry = route.Airliner.CurrentFlight == null ? route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime) : route.Airliner.CurrentFlight.Entry;

                    for (int i = 0; i < route.TimeTable.Entries.Count; i++)
                    {
                        if (!arrivals && entry.Destination.Airport == tAirport)
                            entries.Add(entry);

                        if (arrivals && entry.Destination.Airport == fAirport)
                            entries.Add(entry);

                        entry = route.TimeTable.getNextEntry(entry);
                   
                    }
                  
                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries;
        }
        //returns the list of arrivals for an airport
        public static List<RouteTimeTableEntry> GetAirportArrivals(Airport airport,int count)
        {

            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in airport.Terminals.getRoutes())
            {
                if (route.Airliner != null)
                {
                    RouteTimeTableEntry entry = route.Airliner.CurrentFlight == null ? route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, (airport == route.Destination1 ? route.Destination2 : route.Destination1).Profile.Coordinates) : route.Airliner.CurrentFlight.Entry;

                    for (int i = 0; i < route.TimeTable.Entries.Count; i++)
                    {
                        if (entry.Destination.Airport == airport)
                            entries.Add(entry);
                        entry = route.TimeTable.getNextEntry(entry);
                   
                    }
                   
                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries.GetRange(0, Math.Min(entries.Count, count));
        }
        //returns the list of departures for an airport
        public static List<RouteTimeTableEntry> GetAirportDepartures(Airport airport, int count)
        {
            
            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in airport.Terminals.getRoutes())
            {
                if (route.Airliner != null)
                {
                    RouteTimeTableEntry entry = route.Airliner.CurrentFlight == null ? route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, airport.Profile.Coordinates) : route.Airliner.CurrentFlight.Entry;

                    if (entry.Destination.Airport.Profile.Coordinates.CompareTo(airport.Profile.Coordinates) != 0)
                        entries.Add(entry);

                    while (entries.Count<4)
                    {
                        entry = route.TimeTable.getNextEntry(entry);
                        if (entry.Destination.Airport != airport)
                            entries.Add(entry);
                    }
                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries.GetRange(0, Math.Min(entries.Count, count));
        }
        //returns the rate (for loan) for an airline
        public static double GetAirlineLoanRate(Airline airline)
        {
            double value = (double)airline.getAirlineValue();

            return 1.5 * ((double)Airline.AirlineValue.Very_high + 1-value); 
        }
        //finds all airports in a radius of 1000 km from a airport
        public static List<Airport> GetAirportsNearAirport(Airport airport)
        {
            return Airports.GetAirports().FindAll(a => MathHelpers.GetDistance(airport.Profile.Coordinates, a.Profile.Coordinates) < 1000 && airport != a);
        }
        //creates the big image map
        public static void CreateBigImageCanvas()
        {
            int zoom = 3;
            int imageSize = 256;

            Canvas panelMap = new Canvas();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    string name = string.Format(@"{0}\{1}\{2}.png", zoom, x, y);

                    Image imgMap = new Image();
                    imgMap.Width = imageSize;
                    imgMap.Height = imageSize;
                    imgMap.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\maps\\" + name, UriKind.RelativeOrAbsolute));
                    RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

                    Canvas.SetTop(imgMap, y * imageSize);
                    Canvas.SetLeft(imgMap, x * imageSize);

                    panelMap.Children.Add(imgMap);



                }
            }
            BigMapXaml = XamlWriter.Save(panelMap);


        }
        //converts a range (route distance) to a airliner range type
        public static AirlinerType.TypeRange ConvertDistanceToRangeType(double distance)
        {
            if (distance < 2000) return AirlinerType.TypeRange.Regional;
            if (distance >= 2000 && distance < 3500) return AirlinerType.TypeRange.Short_Range;
            if (distance >= 3500 && distance < 9000) return AirlinerType.TypeRange.Medium_Range;
            if (distance >= 9000) return AirlinerType.TypeRange.Long_Range;

            return AirlinerType.TypeRange.Medium_Range;
        }
        //converts airliner class type to a price factor
        public static int ClassToPriceFactor(AirlinerClass.ClassType type)
        {
            if (type == AirlinerClass.ClassType.Business_Class)
                return 3;
            if (type == AirlinerClass.ClassType.Economy_Class)
                return 1;
            if (type == AirlinerClass.ClassType.First_Class)
                return 6;
            return 1;
        }
        //returns the discount for an order of airliners
        public static double GetAirlinerOrderDiscount(int orders)
        {
            return ((orders - 2) * 0.5)/100;
        }

    }
}
