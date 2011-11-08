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
        //returns the list of arrivals for an airport
        public static List<RouteTimeTableEntry> GetAirportArrivals(Airport airport)
        {

            List<RouteTimeTableEntry> entries = new List<RouteTimeTableEntry>();
            foreach (Route route in airport.Terminals.getRoutes())
            {
                if (route.Airliner != null)
                {
                    RouteTimeTableEntry entry = route.Airliner.CurrentFlight == null ? route.TimeTable.getNextEntry(GameObject.GetInstance().GameTime, (airport == route.Destination1 ? route.Destination2 : route.Destination1).Profile.Coordinates) : route.Airliner.CurrentFlight.Entry;

                    if (entry.Destination.Airport.Profile.Coordinates.CompareTo(airport.Profile.Coordinates) == 0)
                        entries.Add(entry);

                    while (entries.Count<4)
                    {
                        entry = route.TimeTable.getNextEntry(entry);
                        if (entry.Destination.Airport == airport)
                            entries.Add(entry);
                    }
                }
            }
            entries.Sort(delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return MathHelpers.ConvertEntryToDate(e1).CompareTo(MathHelpers.ConvertEntryToDate(e2)); });
            return entries.GetRange(0, Math.Min(entries.Count, 2));
        }
        //returns the list of departures for an airport
        public static List<RouteTimeTableEntry> GetAirportDepartures(Airport airport)
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
            return entries.GetRange(0, Math.Min(entries.Count, 2));
        }
        //returns the rate (for loan) for an airline
        public static double GetAirlineLoanRate(Airline airline)
        {
            double value = (double)airline.getValue();

            return 1.5 * ((double)Airline.AirlineValue.Very_high + 1-value); 
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
    }
}
