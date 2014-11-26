namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageFlights.xaml
    /// </summary>
    public partial class PageFlights : Page
    {
        #region Constructors and Destructors

        public PageFlights()
        {
            this.AllFlights = new ObservableCollection<RouteTimeTableEntry>();

            var flights = Airlines.GetAllAirlines().SelectMany(a => a.Routes.SelectMany(r => r.TimeTable.Entries)).OrderBy(e=>e.Time);
            
            foreach (RouteTimeTableEntry flight in flights)
                this.AllFlights.Add(flight);
                    
            this.AllAirlines = new ObservableCollection<Airline>(Airlines.GetAllAirlines());

            var dummyAirline = new Airline(
              new AirlineProfile("All Airlines", "99", "Blue", "", false, 1900, 1900),
              Airline.AirlineMentality.Safe,
              Airline.AirlineFocus.Domestic,
              Airline.AirlineLicense.Domestic,
              Route.RouteType.Passenger,
              Airline.AirlineRouteSchedule.Regular);
            dummyAirline.Profile.AddLogo(
                new AirlineLogo(AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png"));

            this.AllAirlines.Insert(0, dummyAirline);

            this.Airline = dummyAirline;

            IEnumerable<Route> routes = Airlines.GetAllAirlines().SelectMany(a => a.Routes);

            this.InitializeComponent();

            this.Loaded += this.PageFlights_Loaded;
        }

        #endregion

        #region Private Properties
        private DayOfWeek Day;
        private Airline Airline;

        #endregion


        #region Public Properties

        public ObservableCollection<RouteTimeTableEntry> AllFlights { get; set; }
        public ObservableCollection<Airline> AllAirlines { get; set; }

        #endregion

        #region Methods
        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Airline = (Airline)((ComboBox)sender).SelectedItem;

            setFlights();
        }
        private void PageFlights_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>()
                        .Where(item => item.Tag.ToString() == GameObject.GetInstance().GameTime.DayOfWeek.ToString())
                        .FirstOrDefault();

                tab_main.SelectedItem = matchingItem;
            }
        }

        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            this.Day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), selection, true);

            setFlights();
        }
        private void setFlights()
        {
            if (this.Airline != null)
            {
                var lvFlights = UIHelpers.FindChild<ListView>(this, "lvFlights");

                if (lvFlights != null)
                {
                    var source = lvFlights.Items as ICollectionView;
                    source.Filter = delegate(object item)
                    {
                        var i = item as RouteTimeTableEntry;
                        return i.Day == this.Day && (this.Airline.Profile.Name == "All Airlines" || i.Airliner.Airliner.Airline == this.Airline);
                    };
                }
            }
        }
        #endregion

       
    }
}