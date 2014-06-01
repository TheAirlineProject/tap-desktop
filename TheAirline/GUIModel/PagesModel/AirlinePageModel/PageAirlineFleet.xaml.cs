namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlineModel.SubsidiaryModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.PilotModel;

    /// <summary>
    ///     Interaction logic for PageAirlineFleet.xaml
    /// </summary>
    public partial class PageAirlineFleet : Page
    {
        #region Constructors and Destructors

        public PageAirlineFleet(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;
            this.SelectedAirliners = new ObservableCollection<FleetAirliner>();

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        public ObservableCollection<FleetAirliner> SelectedAirliners { get; set; }

        #endregion

        #region Methods

        private void btnEditAirliners_Click(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageAirlineEditAirliners(this.SelectedAirliners.ToList()) { Tag = this.Tag });
            }
        }

        private void btnRouteMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(this.Airline.Airline.Routes);
        }
        private void btnMoveAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((Button)sender).Tag;
            
            ComboBox cbAirlines = new ComboBox();
            cbAirlines.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAirlines.ItemTemplate = Application.Current.Resources["AirlineItem"] as DataTemplate;
            cbAirlines.HorizontalAlignment = HorizontalAlignment.Left;
            cbAirlines.Width = 200;

            if (this.Airline.Airline.Subsidiaries.Count > 0)
                foreach (SubsidiaryAirline sAirline in this.Airline.Airline.Subsidiaries)
                    cbAirlines.Items.Add(sAirline);
            else
                cbAirlines.Items.Add(((SubsidiaryAirline)this.Airline.Airline).Airline);

            cbAirlines.SelectedIndex = 0;

            if (
                PopUpSingleElement.ShowPopUp(
                    Translator.GetInstance().GetString("PageAirlineFleet", "1016"),
                    cbAirlines) == PopUpSingleElement.ButtonSelected.OK && cbAirlines.SelectedItem != null)
            {
                var airline = cbAirlines.SelectedItem as Airline;

                this.Airline.moveAirliner(airliner, airline);
            }
        }
        private void btnSellAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((Button)sender).Tag;

            if (airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2122"),
                    string.Format(Translator.GetInstance().GetString("MessageBox", "2122", "message")),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                if (airliner.Purchased == FleetAirliner.PurchasedType.Bought)
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2106"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2106", "message"),
                                airliner.Name),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                            {
                                airliner.removeRoute(route);
                            }
                        }

                        this.Airline.removeAirliner(airliner);

                        AirlineHelpers.AddAirlineInvoice(
                            this.Airline.Airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            airliner.Airliner.getPrice());

                        foreach (Pilot pilot in airliner.Pilots)
                        {
                            pilot.Airliner = null;
                        }

                        airliner.Pilots.Clear();
                    }
                }
                else
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2107"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2107", "message"),
                                airliner.Name),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        if (airliner.HasRoute)
                        {
                            var routes = new List<Route>(airliner.Routes);

                            foreach (Route route in routes)
                            {
                                airliner.removeRoute(route);
                            }
                        }

                        this.Airline.removeAirliner(airliner);

                        foreach (Pilot pilot in airliner.Pilots)
                        {
                            pilot.Airliner = null;
                        }

                        airliner.Pilots.Clear();
                    }
                }
            }
        }

        private void cbAirliner_Checked(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((CheckBox)sender).Tag;

            this.SelectedAirliners.Add(airliner);
        }

        private void cbAirliner_Unchecked(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((CheckBox)sender).Tag;

            this.SelectedAirliners.Remove(airliner);
        }

        #endregion
    }
}