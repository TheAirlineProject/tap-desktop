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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineEmployees.xaml
    /// </summary>
    public partial class PageAirlineEmployees : Page
    {
        public AirlineMVVM Airline { get; set; }
        public PageAirlineEmployees(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;

            this.Airline.resetFees();
        
            InitializeComponent();

       
        }
        private void btnFirePilot_Click(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Button)sender).Tag;

            if (pilot.Airliner == null)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2117"), string.Format(Translator.GetInstance().GetString("MessageBox", "2117", "message"), pilot.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airline.removePilot(pilot);
                }


            }
            else
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2116"), string.Format(Translator.GetInstance().GetString("MessageBox", "2116", "message"), pilot.Profile.Name), WPFMessageBoxButtons.Ok);

            }
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            this.Airline.saveFees();
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            this.Airline.resetFees();
        }
    }
}
