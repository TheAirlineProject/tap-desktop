using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using TheAirline.Models.Airports;
using TheAirline.Models.General.Countries;

namespace TheAirline.GUIModel.PagesModel.AirportsPageModel
{
    /// <summary>
    /// Interaction logic for PageAirportsOverview.xaml
    /// </summary>
    public partial class PageAirportsOverview : Page
    {
        public Dictionary<Country,int> AllAirports { get; set; }
        public PageAirportsOverview()
        {
            AllAirports = new Dictionary<Country, int>();

            var airportCountryGroups =
                from a in Airports.GetAllAirports()
                group a by a.Profile.Country into g
                select new { Country = g.Key, Airports = g };

        

            foreach (var g in airportCountryGroups.OrderByDescending(g => g.Airports.Count()))
                AllAirports.Add(g.Country, g.Airports.Count());
     
            InitializeComponent();
        }
    }
}
