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
        private int Zoom;
        private List<Airport> AirportsList;
        private List<Route> RoutesList;
        private Coordinates ZoomCoordinates;
        private Boolean ShowingAirports;
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

            showMap(airliner.HasRoute ? airliner.RouteAirliner.CurrentPosition : airliner.Homebase.Profile.Coordinates, false);

        }
        public PopUpMap(List<Airport> airports)
            : this(ImageSize * 3)
        {
            this.ShowingAirports = true;
            this.KeyDown += new KeyEventHandler(PopUpMap_KeyDown);
            this.AirportsList = airports;
            this.Width = MapSize + 200;
            this.ZoomCoordinates = new Coordinates(new Coordinate(0, 0, 0, Coordinate.Directions.N), new Coordinate(0, 0, 0, Coordinate.Directions.E)); 
            this.Zoom = 1;
        

            showMap(airports, this.Zoom, this.ZoomCoordinates);

        }

        private void PopUpMap_KeyDown(object sender, KeyEventArgs e)
        {
            int zoomer = 4 - this.Zoom;
            if (e.Key == Key.OemPlus && this.Zoom<3)
            {
                this.Zoom++;
                if (this.ShowingAirports)
                    showMap(this.AirportsList, this.Zoom, this.ZoomCoordinates);
                else
                    showMap(this.RoutesList, this.Zoom, this.ZoomCoordinates);
            }
            if (e.Key == Key.OemMinus && this.Zoom>1)
            {
                this.Zoom--;
                if (this.ShowingAirports)
                    showMap(this.AirportsList, this.Zoom, this.ZoomCoordinates);
                else
                    showMap(this.RoutesList, this.Zoom, this.ZoomCoordinates);
   
            }
            if (e.Key == Key.Left && this.ZoomCoordinates.Longitude.Degrees> -180+(40*zoomer))
            {
                this.ZoomCoordinates = new Coordinates(this.ZoomCoordinates.Latitude, new Coordinate(this.ZoomCoordinates.Longitude.Degrees - 40*zoomer, this.ZoomCoordinates.Longitude.Minutes, this.ZoomCoordinates.Longitude.Seconds, this.ZoomCoordinates.Longitude.Direction));
                if (this.ShowingAirports)
                    showMap(this.AirportsList, this.Zoom, this.ZoomCoordinates);
                else
                    showMap(this.RoutesList, this.Zoom, this.ZoomCoordinates);
   
            }
            if (e.Key == Key.Right && this.ZoomCoordinates.Longitude.Degrees<180-(40*zoomer))
            {
                this.ZoomCoordinates = new Coordinates(this.ZoomCoordinates.Latitude, new Coordinate(this.ZoomCoordinates.Longitude.Degrees + 40*zoomer, this.ZoomCoordinates.Longitude.Minutes, this.ZoomCoordinates.Longitude.Seconds, this.ZoomCoordinates.Longitude.Direction));
                if (this.ShowingAirports)
                    showMap(this.AirportsList, this.Zoom, this.ZoomCoordinates);
                else
                    showMap(this.RoutesList, this.Zoom, this.ZoomCoordinates);
   
            }
            if (e.Key == Key.Up && this.ZoomCoordinates.Latitude.Degrees<90-(30*zoomer))
            {
                this.ZoomCoordinates = new Coordinates(new Coordinate(this.ZoomCoordinates.Latitude.Degrees + 30*zoomer, this.ZoomCoordinates.Latitude.Minutes, this.ZoomCoordinates.Latitude.Seconds, this.ZoomCoordinates.Latitude.Direction),this.ZoomCoordinates.Longitude);
                if (this.ShowingAirports)
                    showMap(this.AirportsList, this.Zoom, this.ZoomCoordinates);
                else
                    showMap(this.RoutesList, this.Zoom, this.ZoomCoordinates);
   
            }
            if (e.Key == Key.Down && this.ZoomCoordinates.Latitude.Degrees>-90 + (30*zoomer))
            {
                this.ZoomCoordinates = new Coordinates(new Coordinate(this.ZoomCoordinates.Latitude.Degrees - 30*zoomer, this.ZoomCoordinates.Latitude.Minutes, this.ZoomCoordinates.Latitude.Seconds, this.ZoomCoordinates.Latitude.Direction), this.ZoomCoordinates.Longitude);
                if (this.ShowingAirports)
                    showMap(this.AirportsList, this.Zoom, this.ZoomCoordinates);
                else
                    showMap(this.RoutesList, this.Zoom, this.ZoomCoordinates);
   
            }
            
        }
        public PopUpMap(List<Route> routes)
            : this(ImageSize * 3)
        {
            this.KeyDown += new KeyEventHandler(PopUpMap_KeyDown);
            this.Zoom = 1;
            this.ZoomCoordinates = new Coordinates(new Coordinate(0, 0, 0, Coordinate.Directions.N), new Coordinate(0, 0, 0, Coordinate.Directions.E));
            this.Width = MapSize + 200;
            this.RoutesList = routes;
          
            showMap(this.RoutesList, this.Zoom, this.ZoomCoordinates);
        }
        //creates the side panel for the airport size and zooming
        private StackPanel createAirportSizeSidePanel()
        {
            StackPanel sidePanel = new StackPanel();
            sidePanel.Margin = new Thickness(5, 0, 0, 0);

            foreach (AirportProfile.AirportSize size in Enum.GetValues(typeof(AirportProfile.AirportSize)))
            {
                WrapPanel panelSize = new WrapPanel();
                panelSize.Margin = new Thickness(0, 5, 0, 5);

                Ellipse eSize = new Ellipse();
                eSize.Width = 20;
                eSize.Height = 20;
                eSize.StrokeThickness = 2;
                eSize.Stroke = Brushes.Black;
                eSize.Fill = new SolidColorBrush(getSizeColor(size));

                panelSize.Children.Add(eSize);

                TextBlock txtSize = new TextBlock();
                txtSize.Text = new TextUnderscoreConverter().Convert(size).ToString();
                txtSize.Margin = new Thickness(5, 0, 0, 0);

                panelSize.Children.Add(txtSize);

                sidePanel.Children.Add(panelSize);
            }
            StackPanel panelZoom = new StackPanel();
            panelZoom.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtMapHeader = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpMap", "1000"));
            txtMapHeader.FontWeight = FontWeights.Bold;
            txtMapHeader.FontSize = 14;
            txtMapHeader.TextDecorations = TextDecorations.Underline;

            panelZoom.Children.Add(txtMapHeader);

            panelZoom.Children.Add(UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpMap", "1001"))); 
            panelZoom.Children.Add(UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpMap","1002")));
            panelZoom.Children.Add(UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpMap", "1003")));
            panelZoom.Children.Add(UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpMap", "1004")));
            panelZoom.Children.Add(UICreator.CreateTextBlock(Translator.GetInstance().GetString("PopUpMap", "1005"))); 

            sidePanel.Children.Add(panelZoom);

            //var regions = airports.Select(a => a.Profile.Country.Region).Distinct();//from a in airports select a.Profile.Country.Region;

            return sidePanel;


        }

        //returns the color for a specific airport size
        private Color getSizeColor(AirportProfile.AirportSize size)
        {
            switch (size)
            {
                case AirportProfile.AirportSize.Large:
                    return Colors.DarkBlue;
                case AirportProfile.AirportSize.Largest:
                    return Colors.DarkRed;
                case AirportProfile.AirportSize.Medium:
                    return Colors.Black;
                case AirportProfile.AirportSize.Small:
                    return Colors.White;
                case AirportProfile.AirportSize.Smallest:
                    return Colors.Gray;
                case AirportProfile.AirportSize.Very_large:
                    return Colors.Yellow;
                case AirportProfile.AirportSize.Very_small:
                    return Colors.Violet;
            }
            return Colors.DarkRed;
        }
        //shows an airport
        private void showAirport(Airport airport, Panel panelMap, int zoom, Point margin)
        {
            Point pos = GraphicsHelpers.WorldToTilePos(airport.Profile.Coordinates, zoom);

            Point p = new Point(pos.X * ImageSize - margin.X * ImageSize, pos.Y * ImageSize - margin.Y * ImageSize);

            if (p.X < panelMap.Width)
                panelMap.Children.Add(createPin(p, airport));
        }
        //shows a route
        private void showRoute(Route route, Panel panelMap, int zoom, Point margin)
        {
            Point pos = GraphicsHelpers.WorldToTilePos(route.Destination1.Profile.Coordinates, zoom);

            Point p = new Point(pos.X * ImageSize - margin.X * ImageSize, pos.Y * ImageSize - margin.Y * ImageSize);

            if (p.X < panelMap.Width)
                panelMap.Children.Add(createPin(p, route.Destination1));

            pos = GraphicsHelpers.WorldToTilePos(route.Destination2.Profile.Coordinates, zoom);

            p = new Point(pos.X * ImageSize - margin.X * ImageSize, pos.Y * ImageSize - margin.Y * ImageSize);

            if (p.X < panelMap.Width)
              panelMap.Children.Add(createPin(p, route.Destination2));

            createRouteLine(route.Destination1, route.Destination2,panelMap, zoom,margin);

        }
        //creates the line between two airports
        private void createRouteLine(Airport a1, Airport a2,Panel panelMap, int zoom, Point margin)
        {
            
            Point pos1 = GraphicsHelpers.WorldToTilePos(a1.Profile.Coordinates, zoom);
            Point pos2 = GraphicsHelpers.WorldToTilePos(a2.Profile.Coordinates, zoom);

            Line line = new Line();
            line.Stroke = new AirlineBrushConverter().Convert(GameObject.GetInstance().HumanAirline) as SolidColorBrush;
            line.X1 = Math.Min(panelMap.Width,pos1.X * ImageSize - margin.X * ImageSize);
            line.X2 = Math.Min(panelMap.Width, pos2.X * ImageSize - margin.X * ImageSize);
            line.Y1 = pos1.Y * ImageSize - margin.Y * ImageSize;
            line.Y2 = pos2.Y * ImageSize - margin.Y * ImageSize;

            panelMap.Children.Add(line);

        }
        public PopUpMap(Airport airport)
            : this(ImageSize)
        {
            showMap(airport.Profile.Coordinates, true);
        }
        //shows the map for a list of routes with specific coordinates in focus
        private void showMap(List<Route> routes, int zoom, Coordinates focused)
        {
            double px, py;

            if (focused != null)
            {
                Point pos = GraphicsHelpers.WorldToTilePos(focused, zoom);

                px = Math.Max(1, pos.X);
                py = Math.Max(1, pos.Y);
            }
            else
            {
                px = 1;
                py = 1;
            }

            Canvas panelMap = new Canvas();

            Canvas panelMainMap = new Canvas();
            panelMainMap.Width = 2 * ImageSize;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    string name = string.Format(@"{0}\{1}\{2}.png", zoom, x - 1 + (int)px, y - 1 + (int)py);

                    Image imgMap = new Image();
                    imgMap.Width = ImageSize;
                    imgMap.Height = ImageSize;
                    imgMap.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\maps\\" + name, UriKind.RelativeOrAbsolute));
                    RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

                    Canvas.SetTop(imgMap, y * ImageSize);
                    Canvas.SetLeft(imgMap, x * ImageSize);

                    panelMainMap.Children.Add(imgMap);



                }
            }
            ScaleTransform transform = new ScaleTransform();
            transform.ScaleX = 1.5;
            transform.ScaleY = 1.5;
            panelMainMap.RenderTransform = transform;

        

            foreach (Route route in routes)
                showRoute(route, panelMainMap, zoom, new Point((int)px - 1, (int)py - 1));

          
            StackPanel sidePanel = createAirportSizeSidePanel();
            Canvas.SetTop(sidePanel, 0);
            Canvas.SetLeft(sidePanel, this.MapSize);

            panelMap.Children.Add(panelMainMap);
            panelMap.Children.Add(sidePanel);

            

            this.Content = panelMap;
        }
        //shows the map for a list of airport with specific coordinates in focus
        private void showMap(List<Airport> airports, int zoom, Coordinates focused)
        {
            double px, py;

            if (focused != null)
            {
                Point pos = GraphicsHelpers.WorldToTilePos(focused, zoom);

                px = Math.Max(1, pos.X);
                py = Math.Max(1, pos.Y);
            }
            else
            {
                px = 1;
                py = 1;
            }

            Canvas panelMap = new Canvas();

            Canvas panelMainMap = new Canvas();
            panelMainMap.Width = 2 * ImageSize;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    string name = string.Format(@"{0}\{1}\{2}.png", zoom, x - 1 + (int)px, y - 1 + (int)py);

                    Image imgMap = new Image();
                    imgMap.Width = ImageSize;
                    imgMap.Height = ImageSize;
                    imgMap.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\maps\\" + name, UriKind.RelativeOrAbsolute));
                    RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

                    Canvas.SetTop(imgMap, y * ImageSize);
                    Canvas.SetLeft(imgMap, x * ImageSize);

                    panelMainMap.Children.Add(imgMap);



                }
            }
            ScaleTransform transform = new ScaleTransform();
            transform.ScaleX = 1.5;
            transform.ScaleY = 1.5;
            panelMainMap.RenderTransform = transform; 

            StackPanel sidePanel = createAirportSizeSidePanel();
            Canvas.SetTop(sidePanel, 0);
            Canvas.SetLeft(sidePanel, this.MapSize);

            panelMap.Children.Add(panelMainMap);
            panelMap.Children.Add(sidePanel);


            foreach (Airport airport in airports)
                showAirport(airport, panelMainMap, zoom, new Point((int)px - 1, (int)py - 1));
            
            this.Content = panelMap;
        }
        //creates the map for coordinates
        private void showMap(Coordinates coordinates, Boolean isAirport)
        {
            this.Zoom = 3;


            Canvas c = new Canvas();
            c.Width = this.Width;
            c.Height = this.Height;


            StringReader stringReader = new StringReader(GeneralHelpers.BigMapXaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            Canvas panelMap = (Canvas)XamlReader.Load(xmlReader);


            Point pos = GraphicsHelpers.WorldToTilePos(coordinates, this.Zoom);

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
                panelMap.Children.Add(createPin(p, Airports.GetAirport(coordinates)));
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
        private UIElement createPin(Point position, Airport airport)
        {
            Ellipse imgPin = new Ellipse();
            imgPin.Tag = airport;
            imgPin.Fill = new SolidColorBrush(getSizeColor(airport.Profile.Size));
            imgPin.Height = 8;
            imgPin.Width = imgPin.Height;
            imgPin.Stroke = Brushes.Black;
            imgPin.StrokeThickness = 1;
            imgPin.MouseDown += new MouseButtonEventHandler(imgPin_MouseDown);


            Border brdToolTip = new Border();
            brdToolTip.Margin = new Thickness(-4, 0, -4, -3);
            brdToolTip.Padding = new Thickness(5);
            brdToolTip.SetResourceReference(Border.BackgroundProperty, "HeaderBackgroundBrush2");


            ContentControl lblAirport = new ContentControl();
            lblAirport.SetResourceReference(ContentControl.ContentTemplateProperty, "AirportCountryItemNormal");
            lblAirport.Content = airport;

            brdToolTip.Child = lblAirport;


            imgPin.ToolTip = brdToolTip;

            Canvas.SetTop(imgPin, position.Y - imgPin.Height + 5);
            Canvas.SetLeft(imgPin, position.X - imgPin.Height / 2);

            return imgPin;

        }

          private void imgPin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Airport airport = (Airport)((Ellipse)sender).Tag;
                PageNavigator.NavigateTo(new PageAirport(airport));

                this.Close();
            }
          

        }
    }
}
