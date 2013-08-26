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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

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
            

           
            btnSet.IsEnabled = cbAirport.Items.Count > 0; 

            cbAirport.SelectedIndex = 0;

            Panel contentPanel = (Panel)this.Content;

            RadioButton rbDateC = UICreator.FindChild<RadioButton>(contentPanel, "rbDateC");
            RadioButton rbDateD = UICreator.FindChild<RadioButton>(contentPanel, "rbDateD");
            RadioButton rbIntervalC = UICreator.FindChild<RadioButton>(contentPanel, "rbIntervalC");
            RadioButton rbIntervalD = UICreator.FindChild<RadioButton>(contentPanel, "rbIntervalD");
         
            Slider slMaintenanceD = UICreator.FindChild<Slider>(contentPanel, "slMaintenanceD");
            Slider slMaintenanceC = UICreator.FindChild<Slider>(contentPanel, "slMaintenanceC");
  
            rbDateC.IsChecked = this.Airliner.CMaintenanceInterval == -1;
            rbDateD.IsChecked = this.Airliner.DMaintenanceInterval == -1;

            rbIntervalC.IsChecked = !rbDateC.IsChecked;
            rbIntervalD.IsChecked = !rbDateD.IsChecked;
              
            if (rbDateC.IsChecked.Value)
                this.dpMaintenanceC.SelectedDate = this.Airliner.SchedCMaintenance;
            else
                slMaintenanceC.Value = this.Airliner.CMaintenanceInterval;

            if (rbDateD.IsChecked.Value)
                this.dpMaintenanceD.SelectedDate = this.Airliner.SchedDMaintenance;
            else
                slMaintenanceD.Value = this.Airliner.DMaintenanceInterval;

            slMaintenanceA.Value = this.Airliner.AMaintenanceInterval;
            slMaintenanceB.Value = this.Airliner.BMaintenanceInterval;
           
        }

        private void btnOK_onClick(object sender, RoutedEventArgs e)
        {
            schedule_Maintenance();
        }

        private void schedule_Maintenance()
        {            
            //sets the values
            int aMI = (int)this.slMaintenanceA.Value;
            this.Airliner.SchedAMaintenance = GameObject.GetInstance().GameTime.AddDays(aMI);
            int bMI = (int)this.slMaintenanceB.Value;
            this.Airliner.SchedBMaintenance = GameObject.GetInstance().GameTime.AddDays(bMI);

            RadioButton rbDateC = UICreator.FindChild<RadioButton>(this, "rbDateC");
            RadioButton rbDateD = UICreator.FindChild<RadioButton>(this, "rbDateD");
            Slider slMaintenanceD = UICreator.FindChild<Slider>(this, "slMaintenanceD");
            Slider slMaintenanceC = UICreator.FindChild<Slider>(this, "slMaintenanceC");

            if (rbDateC.IsChecked == true && rbDateD.IsChecked == true)
                {
                    this.Airliner.SchedCMaintenance = (DateTime)this.dpMaintenanceC.SelectedDate;                    
                    this.Airliner.SchedDMaintenance = (DateTime)this.dpMaintenanceD.SelectedDate;
                    FleetAirlinerHelpers.SetMaintenanceIntervals(this.Airliner, aMI, bMI);
                }
            else if (rbDateC.IsChecked == true && rbDateD.IsChecked == false)
                {
                    this.Airliner.SchedCMaintenance = (DateTime)this.dpMaintenanceC.SelectedDate;
                    int dMI = (int)slMaintenanceD.Value;
                    this.Airliner.CMaintenanceInterval = -1;
                    this.Airliner.SchedDMaintenance = GameObject.GetInstance().GameTime.AddMonths(dMI);
                    FleetAirlinerHelpers.SetMaintenanceIntervals(this.Airliner, aMI, bMI, dMI);
                }
            else if (rbDateD.IsChecked == true && rbDateC.IsChecked == false)
                {
                    this.Airliner.SchedDMaintenance = (DateTime)this.dpMaintenanceD.SelectedDate;
                    this.Airliner.DMaintenanceInterval = -1;
                    int cMI = (int)slMaintenanceC.Value;
                    this.Airliner.SchedCMaintenance = GameObject.GetInstance().GameTime.AddMonths(cMI);
                    FleetAirlinerHelpers.SetMaintenanceIntervals(this.Airliner, aMI, bMI, cMI);

                }
            else
                {
                    int dMI = (int)slMaintenanceD.Value;
                    this.Airliner.SchedDMaintenance = GameObject.GetInstance().GameTime.AddMonths(dMI);
                    int cMI = (int)slMaintenanceC.Value;
                    this.Airliner.SchedCMaintenance = GameObject.GetInstance().GameTime.AddMonths(cMI);
                    FleetAirlinerHelpers.SetMaintenanceIntervals(this.Airliner, aMI, bMI, cMI, dMI);
                }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            setValues();
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            

        }
       
    }
}
