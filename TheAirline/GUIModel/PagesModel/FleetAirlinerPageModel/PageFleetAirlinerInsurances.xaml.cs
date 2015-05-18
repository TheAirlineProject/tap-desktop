using TheAirline.Models.Airports;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageFleetAirlinerInsurances.xaml
    /// </summary>
    public partial class PageFleetAirlinerInsurances : Page
    {
        #region Constructors and Destructors

        public PageFleetAirlinerInsurances(FleetAirlinerMVVM airliner)
        {
            this.Airliner = airliner;
            this.DataContext = this.Airliner;
            this.AllAirports = new List<Airport>();

            IEnumerable<Airport> airports =
                this.Airliner.Airliner.Airliner.Airline.Airports.Where(
                    a =>
                        a.GetAirlineAirportFacility(
                            GameObject.GetInstance().HumanAirline,
                            AirportFacility.FacilityType.Service).Facility.TypeLevel > 1);

            foreach (Airport airport in airports)
            {
                this.AllAirports.Add(airport);
            }

            this.InitializeComponent();

            this.rbDateC.IsChecked = this.Airliner.CMaintenanceInterval == -1;
            this.rbDateD.IsChecked = this.Airliner.DMaintenanceInterval == -1;

            this.rbIntervalC.IsChecked = !this.rbDateC.IsChecked;
            this.rbIntervalD.IsChecked = !this.rbDateD.IsChecked;

            if (this.rbDateC.IsChecked.Value)
            {
                this.dpMaintenanceC.SelectedDate = this.Airliner.SchedCMaintenance;
            }
            else
            {
                this.slMaintenanceC.Value = this.Airliner.CMaintenanceInterval;
            }

            if (this.rbDateD.IsChecked.Value)
            {
                this.dpMaintenanceD.SelectedDate = this.Airliner.SchedDMaintenance;
            }
            else
            {
                this.slMaintenanceD.Value = this.Airliner.DMaintenanceInterval;
            }
        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        public List<Airport> AllAirports { get; set; }

        #endregion
    }
}