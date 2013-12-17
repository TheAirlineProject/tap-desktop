using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    public class PopUpNewAirlinerHomebase : PopUpWindow
    {
        private FleetAirliner Airliner;
        private ComboBox cbAirport;
        public static object ShowPopUp(FleetAirliner airliner)
        {
            PopUpNewAirlinerHomebase window = new PopUpNewAirlinerHomebase(airliner);
            window.ShowDialog();

            return window.Selected == null ? null : window.Selected;
        }
        public PopUpNewAirlinerHomebase(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            this.Title = string.Format("Select new homebase for {0}", airliner.Name);

            this.Width = 300;

            this.Height = 100;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);
            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.IsSynchronizedWithCurrentItem = true;
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Airport> airports = this.Airliner.Airliner.Airline.Airports.FindAll(a => a.getCurrentAirportFacility(this.Airliner.Airliner.Airline, AirportFacility.FacilityType.Service).TypeLevel > 0 && a.Profile.Period.From <= GameObject.GetInstance().GameTime && a.Profile.Period.To > GameObject.GetInstance().GameTime);

            if (airports.Count == 0)
                airports = this.Airliner.Airliner.Airline.Airports.FindAll(a => a.Profile.Period.From <= GameObject.GetInstance().GameTime && a.Profile.Period.To > GameObject.GetInstance().GameTime);

            airports.Sort(delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); });

            foreach (Airport airport in airports)
                cbAirport.Items.Add(airport);

            cbAirport.SelectedIndex = 0;

            mainPanel.Children.Add(cbAirport);

            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;


        }
        //creates the button panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = "OK";
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            panelButtons.Children.Add(btnOk);


            return panelButtons;
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)cbAirport.SelectedItem;
            this.Selected = airport;

            if (((Airport)this.Selected).getCurrentAirportFacility(this.Airliner.Airliner.Airline, AirportFacility.FacilityType.Service).TypeLevel == 0)
            {
                AirportFacility facility = Hub.MinimumServiceFacility;
                airport.addAirportFacility(this.Airliner.Airliner.Airline, facility, GameObject.GetInstance().GameTime.AddDays(facility.BuildingDays));

                double price = facility.Price;

                if (airport.Profile.Country != this.Airliner.Airliner.Airline.Profile.Country)
                    price = price * 1.25;

                AirlineHelpers.AddAirlineInvoice(this.Airliner.Airliner.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -price);
            }

            this.Close();
        }

    }
}
