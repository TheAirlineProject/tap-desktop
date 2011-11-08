using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpMap.xaml
    /// </summary>
    public partial class PopUpMap : PopUpWindow
    {
        private int MapSize;
        private const int ImageSize = 256;
        //shows the pop up for an airport
        public static void ShowPopUp(Airport airport)
        {
            PopUpMap window = new PopUpMap(airport);
            window.ShowDialog();
        }
        //shows the pop up for an airliner
        public static void ShowPopUp(FleetAirliner airliner)
        {
            PopUpMap window = new PopUpMap(airliner);
            window.ShowDialog();
        }
        //shows the pop up for some routes
        public static void ShowPopUp(List<Route> routes)
        {
            PopUpMap window = new PopUpMap(routes);
            window.ShowDialog();
        }
        //shows the pop for a list of airports
        public static void ShowPopUp(List<Airport> airports)
        {
            PopUpMap window = new PopUpMap(airports);
            window.ShowDialog();
        }
        public static void ShowPopUp(Route route)
        {
            List<Route> routes = new List<Route>();
            routes.Add(route);

            ShowPopUp(routes);
        }
        public PopUpMap(int mapSize)
        {
            InitializeComponent();
            this.Uid = "1000";

            this.Title = Translator.GetInstance().GetString("PopUpMap", this.Uid);

            this.Width = mapSize;

            this.Height = mapSize;

            this.MapSize = mapSize;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

        }
        public PopUpMap(FleetAirliner airliner)
            : this(ImageSize)
        {
          
            showMap(airliner.HasRoute ? airliner.RouteAirliner.CurrentPosition : airliner.Homebase.Profile.Coordinates,false);

        }
        public PopUpMap(List<Airport> airports)
            : this(ImageSize * 2)
        {
            int zoom = 1;

            Canvas panelMap = new Canvas();

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    string name = string.Format(@"{0}\{1}\{2}.png", zoom, x, y);

                    Image imgMap = new Image();
                    imgMap.Width = ImageSize;
                    imgMap.Height = ImageSize;
                    imgMap.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\maps\\" + name, UriKind.RelativeOrAbsolute));
                    RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

                    Canvas.SetTop(imgMap, y * ImageSize);
                    Canvas.SetLeft(imgMap, x * ImageSize);

                    panelMap.Children.Add(imgMap);



                }
            }

            foreach (Airport airport in airports)
                showAirport(airport, panelMap, zoom, airports.Count < 10);
            this.Content = panelMap;
        }
        public PopUpMap(List<Route> routes)
            : this(ImageSize * 2)
        {
            int zoom = 1;

            Canvas panelMap = new Canvas();

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    string name = string.Format(@"{0}\{1}\{2}.png", zoom, x, y);

                    Image imgMap = new Image();
                    imgMap.Width = ImageSize;
                    imgMap.Height = ImageSize;
                    imgMap.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\maps\\" + name, UriKind.RelativeOrAbsolute));
                    RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

                    Canvas.SetTop(imgMap, y * ImageSize);
                    Canvas.SetLeft(imgMap, x * ImageSize);

                    panelMap.Children.Add(imgMap);



                }
            }

            foreach (Route route in routes)
                 showRoute(route, panelMap, zoom);
            this.Content = panelMap;
        }
        //shows an airport
        private void showAirport(Airport airport, Panel panelMap, int zoom, Boolean largeSize)
        {
            Point pos = GraphicsHelpers.WorldToTilePos(airport.Profile.Coordinates, zoom);

            Point p = new Point(pos.X * ImageSize, pos.Y * ImageSize);

            panelMap.Children.Add(createPin(p, airport, largeSize));
        }
        //shows a route
        private void showRoute(Route route, Panel panelMap, int zoom)
        {
            Point pos = GraphicsHelpers.WorldToTilePos(route.Destination1.Profile.Coordinates, zoom);

            Point p = new Point(pos.X * ImageSize, pos.Y * ImageSize);

            panelMap.Children.Add(createPin(p,route.Destination1,true));

            pos = GraphicsHelpers.WorldToTilePos(route.Destination2.Profile.Coordinates, zoom);

            p = new Point(pos.X * ImageSize, pos.Y * ImageSize);

            panelMap.Children.Add(createPin(p,route.Destination2,true));

            panelMap.Children.Add(createRouteLine(route.Destination1, route.Destination2, zoom));

        }
        //creates the line between two airports
        private Line createRouteLine(Airport a1, Airport a2, int zoom)
        {
            Point pos1 = GraphicsHelpers.WorldToTilePos(a1.Profile.Coordinates, zoom);
            Point pos2 = GraphicsHelpers.WorldToTilePos(a2.Profile.Coordinates, zoom);

            Line line = new Line();
            line.Stroke = new AirlineBrushConverter().Convert(GameObject.GetInstance().HumanAirline) as SolidColorBrush;
            line.X1 = pos1.X * ImageSize;
            line.X2 = pos2.X * ImageSize;
            line.Y1 = pos1.Y * ImageSize;
            line.Y2 = pos2.Y * ImageSize;

            return line;

        }
        public PopUpMap(Airport airport)
            : this(ImageSize)
        {
            showMap(airport.Profile.Coordinates,true);
        }
       
        //creates the map for coordinates
        private void showMap(Coordinates coordinates, Boolean isAirport)
        {
            int zoom = 3;


            Canvas c = new Canvas();
            c.Width = this.Width;
            c.Height = this.Height;


            StringReader stringReader = new StringReader(GeneralHelpers.BigMapXaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            Canvas panelMap = (Canvas)XamlReader.Load(xmlReader);

           
            Point pos = GraphicsHelpers.WorldToTilePos(coordinates, zoom);

            Point p = new Point(pos.X * ImageSize, pos.Y * ImageSize);


            double tMapSize = Math.Sqrt(panelMap.Children.Count) * ImageSize;

            double x = p.X - this.Width / 2;
            double y = p.Y - this.Height / 2;

            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x + this.Width > tMapSize) x = tMapSize - this.Width;
            if (y + this.Height > tMapSize) y = tMapSize - this.Height;

            //panelMap.ClipToBounds = true;

            panelMap.Width = this.Width;
            panelMap.Height = this.Height;

            if (isAirport)
                panelMap.Children.Add(createPin(p, Airports.GetAirport(coordinates),true));
            else
                panelMap.Children.Add(createPin(p));


            panelMap.Clip = new RectangleGeometry(new Rect(x, y, this.Width, this.Height));
            // panelMap.ClipToBounds = false;

            Canvas.SetLeft(panelMap, -x);
            Canvas.SetTop(panelMap, -y);
            c.Children.Add(panelMap);
            c.ClipToBounds = true;

            this.Content = c;
        }
        //creates the pin at  a position for an airliner
        private Image createPin(Point position)
        {
            Image imgPin = new Image();
            imgPin.Source = new BitmapImage(new Uri(@"/Data/images/airplanepin.png", UriKind.RelativeOrAbsolute));

            imgPin.Height = 24;
            RenderOptions.SetBitmapScalingMode(imgPin, BitmapScalingMode.HighQuality);

            Canvas.SetTop(imgPin, position.Y - imgPin.Height + 5);
            Canvas.SetLeft(imgPin, position.X - imgPin.Height / 2);

            return imgPin;

        }
        //creates the pin at a position with airport
        private Image createPin(Point position,Airport airport, Boolean largeSize)
        {
            Image imgPin = new Image();
            imgPin.Source = new BitmapImage(new Uri(largeSize ? @"/Data/images/pin.png" : @"/Data/images/circle.png", UriKind.RelativeOrAbsolute));
            imgPin.Tag = airport;
            imgPin.MouseDown += new MouseButtonEventHandler(imgPin_MouseDown);

            Border brdToolTip = new Border();
            brdToolTip.Margin = new Thickness(-4,0,-4,-3);
            brdToolTip.Padding = new Thickness(5);
            brdToolTip.SetResourceReference(Border.BackgroundProperty, "HeaderBackgroundBrush2");
            

            ContentControl lblAirport = new ContentControl();
            lblAirport.SetResourceReference(ContentControl.ContentTemplateProperty, "AirportCountryItemNormal");
            lblAirport.Content = airport;

            brdToolTip.Child = lblAirport;


            imgPin.ToolTip = brdToolTip;

           


            imgPin.Height = largeSize ? 24 : 8;
            RenderOptions.SetBitmapScalingMode(imgPin, BitmapScalingMode.HighQuality);

            Canvas.SetTop(imgPin, position.Y - imgPin.Height + 5);
            Canvas.SetLeft(imgPin, position.X - imgPin.Height / 2);
       
            return imgPin;
     
        }

        private void imgPin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Airport airport = (Airport)((Image)sender).Tag;
            PageNavigator.NavigateTo(new PageAirport(airport));

            this.Close();
        }
    }
}
