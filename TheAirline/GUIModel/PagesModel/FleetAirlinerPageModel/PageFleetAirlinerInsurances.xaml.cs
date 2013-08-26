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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    /// <summary>
    /// Interaction logic for PageFleetAirlinerInsurances.xaml
    /// </summary>
    public partial class PageFleetAirlinerInsurances : Page
    {
        public FleetAirlinerMVVM Airliner { get; set; }
        public List<Airport> AllAirports { get; set; }
        public PageFleetAirlinerInsurances(FleetAirlinerMVVM airliner)
        {
            this.Airliner = airliner;
            this.DataContext = this.Airliner;
            this.AllAirports = new List<Airport>();

            var airports = this.Airliner.Airliner.Airliner.Airline.Airports.Where(a => a.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).Facility.TypeLevel > 1);

            foreach (Airport airport in airports)
                this.AllAirports.Add(airport);

            InitializeComponent();
        }
    }
}
