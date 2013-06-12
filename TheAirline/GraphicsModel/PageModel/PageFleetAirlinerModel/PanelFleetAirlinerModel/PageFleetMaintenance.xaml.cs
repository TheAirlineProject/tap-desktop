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
            Schedule_Maintenance();
        }

        private void Schedule_Maintenance()
        {            
            //sets the
            int aMI = (int)this.slMaintenanceA.Value;
            this.Airliner.SchedAMaintenance = GameObject.GetInstance().GameTime.AddDays(aMI);
            int bMI = (int)this.slMaintenanceB.Value;
            this.Airliner.SchedBMaintenance = GameObject.GetInstance().GameTime.AddDays(bMI);

            if (rbDateC.IsChecked == true && rbDateD.IsChecked == true)
                {
                    this.Airliner.SchedCMaintenance = (DateTime)this.dpMaintenanceC.SelectedDate;                    
                    this.Airliner.SchedDMaintenance = (DateTime)this.dpMaintenanceD.SelectedDate;
                    this.Airliner.SetMaintenanceIntervals(this.Airliner, aMI, bMI);
                }
            else if (rbDateC.IsChecked == true && rbDateD.IsChecked == false)
                {
                    this.Airliner.SchedCMaintenance = (DateTime)this.dpMaintenanceC.SelectedDate;
                    int dMI = (int)this.slMaintenanceD.Value;
                    this.Airliner.CMaintenanceInterval = -1;
                    this.Airliner.SchedDMaintenance = GameObject.GetInstance().GameTime.AddMonths(dMI);
                    this.Airliner.SetMaintenanceIntervals(this.Airliner, aMI, bMI, dMI);
                }
            else if (rbDateD.IsChecked == true && rbDateC.IsChecked == false)
                {
                    this.Airliner.SchedDMaintenance = (DateTime)this.dpMaintenanceD.SelectedDate;
                    this.Airliner.DMaintenanceInterval = -1;
                    int cMI = (int)this.slMaintenanceC.Value;
                    this.Airliner.SchedCMaintenance = GameObject.GetInstance().GameTime.AddMonths(cMI);
                    this.Airliner.SetMaintenanceIntervals(this.Airliner, aMI, bMI, cMI);

                }
            else
                {
                    int dMI = (int)this.slMaintenanceD.Value;
                    this.Airliner.SchedDMaintenance = GameObject.GetInstance().GameTime.AddMonths(dMI);
                    int cMI = (int)this.slMaintenanceC.Value;
                    this.Airliner.SchedCMaintenance = GameObject.GetInstance().GameTime.AddMonths(cMI);
                    this.Airliner.SetMaintenanceIntervals(this.Airliner, aMI, bMI, cMI, dMI);
                }
        }
    }
}
