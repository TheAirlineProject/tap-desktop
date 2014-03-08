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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFleet.xaml
    /// </summary>
    public partial class PageAirlineFleet : Page
    {
        public AirlineMVVM Airline { get; set; }
        public PageAirlineFleet(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;

            InitializeComponent();
        }
        private void btnSellAirliner_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Button)sender).Tag;

            if (airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2122"), string.Format(Translator.GetInstance().GetString("MessageBox", "2122", "message")), WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (airliner.Purchased == FleetAirliner.PurchasedType.Bought)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2106"), string.Format(Translator.GetInstance().GetString("MessageBox", "2106", "message"), airliner.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                                airliner.removeRoute(route);
                        }

                        this.Airline.removeAirliner(airliner);

                        AirlineHelpers.AddAirlineInvoice(this.Airline.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, airliner.Airliner.getPrice());


                        foreach (Pilot pilot in airliner.Pilots)
                            pilot.Airliner = null;

                        airliner.Pilots.Clear();


                    }
                }
                else
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2107"), string.Format(Translator.GetInstance().GetString("MessageBox", "2107", "message"), airliner.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                                airliner.removeRoute(route);
                        }


                        this.Airline.removeAirliner(airliner);

                        foreach (Pilot pilot in airliner.Pilots)
                            pilot.Airliner = null;

                        airliner.Pilots.Clear();

                    }
                }

            }

        }
      
    }
}
