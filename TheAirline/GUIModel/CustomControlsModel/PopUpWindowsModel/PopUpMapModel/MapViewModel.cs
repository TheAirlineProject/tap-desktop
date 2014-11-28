using MapControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel.PopUpMapModel
{
    public class MapViewModel
    {
        public ObservableCollection<VmPolyline> Routes { get; set; }
        public ObservableCollection<VmPoint> Airports { get; set; }
        public Location Center { get; set; }
        public MapViewModel(List<Airport> airports = null, List<Route> routes = null)
        {

            this.Routes = new ObservableCollection<VmPolyline>();
            this.Airports = new ObservableCollection<VmPoint>();

            if (routes != null)
            {
                var allRoutes = new List<Route>();

                foreach (Route route in routes)
                {
                    if (route.HasStopovers)
                        foreach (StopoverRoute sRoute in routes.SelectMany(r => r.Stopovers))
                            foreach (Route leg in sRoute.Legs)
                                allRoutes.Add(leg);
                    else
                        allRoutes.Add(route);
                }


              

                foreach (Route route in allRoutes)
                {
                    this.Airports.Add(
             new VmPoint
             {
                 Airport = route.Destination1,
                 Location = new Location(route.Destination1.Profile.Coordinates.Latitude.getAsDecimal(), route.Destination1.Profile.Coordinates.Longitude.getAsDecimal())
             });
                    this.Airports.Add(
             new VmPoint
             {
                 Airport = route.Destination2,
                 Location = new Location(route.Destination2.Profile.Coordinates.Latitude.getAsDecimal(), route.Destination2.Profile.Coordinates.Longitude.getAsDecimal())
             });
                    var locations = new List<Location>();
                    locations.Add(new Location(route.Destination2.Profile.Coordinates.Latitude.getAsDecimal(), route.Destination2.Profile.Coordinates.Longitude.getAsDecimal()));
                    locations.Add(new Location(route.Destination1.Profile.Coordinates.Latitude.getAsDecimal(), route.Destination1.Profile.Coordinates.Longitude.getAsDecimal()));

                    this.Routes.Add(
              new VmPolyline
              {
                  
                  Locations = new LocationCollection(locations)
              });
                }
            }
         
            if (airports != null)
            {
                foreach (Airport airport in airports)
                {
                                    
                    this.Airports.Add(
               new VmPoint
               {
                   Airport = airport,
                   Location = new Location(airport.Profile.Coordinates.Latitude.getAsDecimal(), airport.Profile.Coordinates.Longitude.getAsDecimal())
               });
                }
            }

            if (this.Airports.Count > 0)
                this.Center = new Location(this.Airports[0].Airport.Profile.Coordinates.Latitude.getAsDecimal(), this.Airports[0].Airport.Profile.Coordinates.Longitude.getAsDecimal());



        }
    }
    public class VmPolyline
    {
        public LocationCollection Locations { get; set; }
    }
    public class VmPoint
    {
        public Airport Airport { get; set; }

        private Location location;
        public Location Location
        {
            get { return location; }
            set
            {
                location = value;

            }
        }
    }
    public class MapTileSource : ImageTileSource
    {
        public MapTileSource()
        {

        }


        public override System.Windows.Media.ImageSource LoadImage(int x, int y, int zoomLevel)
        {
            string name = string.Format(@"{0}\{1}\{2}.png", zoomLevel,x,y);

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(AppSettings.getDataPath() + "\\graphics\\maps\\" + name);//sampleImages/cherries_larger.jpg", UriKind.RelativeOrAbsolute);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();

            return bi;
        }
    }
    public class PinColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Airport airport = (Airport)value;

            string color="";

            switch (airport.Profile.Size)
            {
                case GeneralHelpers.Size.Largest:
                    color = "orange";
                    break;
                case GeneralHelpers.Size.Very_large:
                    color = "black";
                    break;
                case GeneralHelpers.Size.Large:
                    color = "green";
                    break;
                case GeneralHelpers.Size.Medium:
                    color = "red";
                    break;
                case GeneralHelpers.Size.Small:
                    color = "gray";
                    break;
                case GeneralHelpers.Size.Very_small:
                    color = "purple";
                    break;
                case GeneralHelpers.Size.Smallest:
                    color = "blue";
                    break;

            }

            return AppSettings.getDataPath() + "\\graphics\\maps\\pins\\" + color + ".png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LocationToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Hidden;

            if (values.Length == 2 && values[0] is Map && values[1] is Transform)
            {
                var parentMap = (Map)values[0];
                var transform = ((Transform)values[1]).Value;

                if (transform.OffsetX >= 0d && transform.OffsetX <= parentMap.ActualWidth &&
                    transform.OffsetY >= 0d && transform.OffsetY <= parentMap.ActualHeight)
                {
                    visibility = Visibility.Visible;
                }
            }

            return visibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
