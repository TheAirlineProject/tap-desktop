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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    /// Interaction logic for PageShowPilot.xaml
    /// </summary>
    public partial class PageShowPilot : Page
    {
        public Pilot Pilot { get; set; }
        public double Salary { get; set; }
        public PageShowPilot(Pilot pilot)
        {
            this.Pilot = pilot;

            InitializeComponent();

            double pilotBasePrice = GameObject.GetInstance().HumanAirline.Fees.getValue(FeeTypes.GetType("Pilot Base Salary"));//GeneralHelpers.GetInflationPrice(133.53);<
            this.Salary = ((int)this.Pilot.Rating) * pilotBasePrice;
            
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Pilots")
       .FirstOrDefault();

                tab_main.SelectedItem = matchingItem;
            }
        }
        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2801"), Translator.GetInstance().GetString("MessageBox", "2801", "message"), WPFMessageBoxButtons.YesNo);


            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.addPilot(this.Pilot);

               
            }

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Pilots")
       .FirstOrDefault();

                tab_main.SelectedItem = matchingItem;
            }
        }
    }
}
