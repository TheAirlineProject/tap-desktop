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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetMaintenance.xaml
    /// </summary>
    public partial class PageFleetMaintenance : Page
    {
        private FleetAirliner Airliner;
        public PageFleetMaintenance(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            InitializeComponent();

            setValues();
       
        }
        //sets the values
        private void setValues()
        {
            cbMaintenanceType.Items.Clear();
            cbMaintenanceType.Items.Add("C");
            cbMaintenanceType.Items.Add("D");
            cbMaintenanceType.SelectedIndex = 0;

            cbAirport.Items.Clear();


            var airports = this.Airliner.Airliner.Airline.Airports.Where(a=>a.getAirlineAirportFacility(GameObject.GetInstance().HumanAirline,AirportFacility.FacilityType.Service).Facility.TypeLevel>1);

            foreach (Airport airport in airports)
                cbAirport.Items.Add(airport);

            btnSet.IsEnabled = cbAirport.Items.Count > 0; 

            cbAirport.SelectedIndex = 0;
        }

        private void btnOK_onClick(object sender, RoutedEventArgs e)
        {

            //sets the dates to the airliner's scheduled maintenance
            int aMaintInterval = (int)this.slMaintenanceA.Value;
            this.Airliner.Airliner.SchedAMaintenance = GameObject.GetInstance().GameTime.AddDays(aMaintInterval);
            int bMaintInterval = (int)this.slMaintenanceB.Value;
            this.Airliner.Airliner.SchedBMaintenance = GameObject.GetInstance().GameTime.AddDays(bMaintInterval);
            this.Airliner.Airliner.SchedCMaintenance = (DateTime)this.dpMaintenanceC.SelectedDate;
            this.Airliner.Airliner.SchedDMaintenance = (DateTime)this.dpMaintenanceD.SelectedDate;
            this.Airliner.Airliner.SetMaintenanceIntervals(this.Airliner.Airliner, aMaintInterval, bMaintInterval);
        }
    }
}
