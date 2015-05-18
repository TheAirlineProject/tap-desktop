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
using TheAirline.Model.GeneralModel;
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
            this.AllAirports = new Dictionary<Country, int>();

            var airportCountryGroups =
                from a in Airports.GetAllAirports()
                group a by a.Profile.Country into g
                select new { Country = g.Key, Airports = g };

        

            foreach (var g in airportCountryGroups.OrderByDescending(g => g.Airports.Count()))
                this.AllAirports.Add(g.Country, g.Airports.Count());
     
            InitializeComponent();
        }
    }
}
