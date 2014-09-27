namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

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
                        a.getAirlineAirportFacility(
                            GameObject.GetInstance().HumanAirline,
                            AirportFacility.FacilityType.Service).Facility.TypeLevel > 1);

            foreach (Airport airport in airports)
            {
                this.AllAirports.Add(airport);
            }

            this.InitializeComponent();

        }

        #endregion

        #region Public Properties

        public FleetAirlinerMVVM Airliner { get; set; }

        public List<Airport> AllAirports { get; set; }

        #endregion

        #region Methods
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            foreach (FleetAirlinerMaintenanceMVVM maintenance in Airliner.Maintenances)
            {
                var m = this.Airliner.Airliner.Maintenance.Checks.FirstOrDefault(c => c.Type == maintenance.Type);

                if (m != null)
                {
                    if (maintenance.Center != null)
                    {
                        m.CheckCenter = new AirlinerMaintenanceCenter(maintenance.Type);

                        if (maintenance.Center.Airport != null)
                            m.CheckCenter.Airport = maintenance.Center.Airport;
                        else
                            m.CheckCenter.Center = maintenance.Center.Center;
                    }

                    m.Interval = maintenance.Interval;
                }

            }
        }
        private void slMaintenance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slMaintenance = (Slider)sender;
            FleetAirlinerMaintenanceMVVM maintenance = (FleetAirlinerMaintenanceMVVM)slMaintenance.Tag;

            maintenance.NextCheck = maintenance.LastCheck.AddDays(slMaintenance.Value);
        }
        private void btnDoMaintenance_Click(object sender, RoutedEventArgs e)
        {
            FleetAirlinerMaintenanceMVVM maintenance = (FleetAirlinerMaintenanceMVVM)((Button)sender).Tag;

            double wage = maintenance.Center.Airport == null ? GeneralHelpers.GetInflationPrice(maintenance.Center.Center.Wage) : GameObject.GetInstance().HumanAirline.Fees.getValue(FeeTypes.GetType("Maintenance Wage"));

            double price = wage * maintenance.Type.Worktime.TotalHours;

            AirlineHelpers.AddAirlineInvoice(this.Airliner.Airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, -GeneralHelpers.GetInflationPrice(price));

            this.Airliner.Airliner.GroundedToDate = GameObject.GetInstance().GameTime.Add(maintenance.Type.Worktime);

            this.Airliner.Airliner.Maintenance.setLastCheck(maintenance.Type, GameObject.GetInstance().GameTime);
            this.Airliner.Airliner.Airliner.LastServiceCheck = this.Airliner.Airliner.Airliner.Flown;

            int deltaCondition = maintenance.Center.Airport == null ? maintenance.Center.Center.Reputation : 50;

            this.Airliner.Airliner.Airliner.Condition = Math.Min(100, this.Airliner.Airliner.Airliner.Condition + deltaCondition);

            maintenance.LastCheck = this.Airliner.Airliner.Maintenance.getLastCheck(maintenance.Type);
            maintenance.NextCheck = maintenance.LastCheck.AddDays(maintenance.Interval);
            maintenance.CanPerformCheck = false;
        }
        #endregion

       

       
    }
}