using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using TheAirline.Models.Airports;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    /// <summary>
    ///     Interaction logic for PageFleetAirlinerInsurances.xaml
    /// </summary>
    public partial class PageFleetAirlinerInsurances : Page
    {
        #region Constructors and Destructors

        public PageFleetAirlinerInsurances(FleetAirlinerMVVM airliner)
        {
            Airliner = airliner;
            DataContext = Airliner;
            AllAirports = new List<Airport>();

            IEnumerable<Airport> airports =
                Airliner.Airliner.Airliner.Airline.Airports.Where(
                    a =>
                        a.GetAirlineAirportFacility(
                            GameObject.GetInstance().HumanAirline,
                            AirportFacility.FacilityType.Service).Facility.TypeLevel > 1);

            foreach (Airport airport in airports)
            {
                AllAirports.Add(airport);
            }

            InitializeComponent();

            rbDateC.IsChecked = Airliner.CMaintenanceInterval == -1;
            rbDateD.IsChecked = Airliner.DMaintenanceInterval == -1;

            rbIntervalC.IsChecked = !rbDateC.IsChecked;
            rbIntervalD.IsChecked = !rbDateD.IsChecked;

            if (rbDateC.IsChecked.Value)
            {
                dpMaintenanceC.SelectedDate = Airliner.SchedCMaintenance;
            }
            else
            {
                slMaintenanceC.Value = Airliner.CMaintenanceInterval;
            }

            if (rbDateD.IsChecked.Value)
            {
                dpMaintenanceD.SelectedDate = Airliner.SchedDMaintenance;
            }
            else
            {
                slMaintenanceD.Value = Airliner.DMaintenanceInterval;
            }
        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        public List<Airport> AllAirports { get; set; }

        #endregion
    }
}