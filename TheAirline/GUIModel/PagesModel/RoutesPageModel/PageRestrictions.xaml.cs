using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PassengerModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageRestrictions.xaml
    /// </summary>
    public partial class PageRestrictions : Page
    {
        public List<FlightRestriction> Restrictions { get; set; }
        public List<BannedAirlinesMVVM> BannedAirlines { get; set; }
        public PageRestrictions()
        {
            this.Loaded += PageRestrictions_Loaded;
            
            this.Restrictions =
              FlightRestrictions.GetRestrictions()
                  .FindAll(
                      r =>
                          (r.Type == FlightRestriction.RestrictionType.Airline || r.Type == FlightRestriction.RestrictionType.Aircrafts || r.Type == FlightRestriction.RestrictionType.Flights || r.Type == FlightRestriction.RestrictionType.Maintenance)
                          && r.StartDate < GameObject.GetInstance().GameTime
                          && r.EndDate > GameObject.GetInstance().GameTime);

            this.BannedAirlines = new List<BannedAirlinesMVVM>();

            var bannedCountries = FlightRestrictions.GetRestrictions()
                  .FindAll(
                      r =>
                          (r.Type == FlightRestriction.RestrictionType.Airlines)
                          && r.StartDate < GameObject.GetInstance().GameTime
                          && r.EndDate > GameObject.GetInstance().GameTime);

            foreach (FlightRestriction bannedCountry in bannedCountries)
            {
                var bannedAirlines = new List<Airline>();
                
                foreach (Airline airline in Airlines.GetAirlines(c => c.Profile.Country == bannedCountry.From))
                    if (!FlightRestrictions.IsAllowed(airline, bannedCountry.To, GameObject.GetInstance().GameTime))
                        bannedAirlines.Add(airline);

                BannedAirlinesMVVM bannedObject = new BannedAirlinesMVVM(bannedCountry.From as Country,bannedCountry.To,bannedAirlines);

                this.BannedAirlines.Add(bannedObject);
            }
            
            InitializeComponent();
        }

        private void PageRestrictions_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Route").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                TabItem airlinerItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                airlinerItem.Visibility = Visibility.Collapsed;
            }
        }
    }
  
}
