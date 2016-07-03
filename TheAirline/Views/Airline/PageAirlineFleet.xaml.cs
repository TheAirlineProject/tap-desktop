using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.Subsidiary;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;
using TheAirline.ViewModels.Airline;
using TheAirline.Views.Airline;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    ///     Interaction logic for PageAirlineFleet.xaml
    /// </summary>
    public partial class PageAirlineFleet : Page
    {
        #region Constructors and Destructors

        public PageAirlineFleet(AirlineMVVM airline)
        {
            Airline = airline;
            DataContext = Airline;
            SelectedAirliners = new ObservableCollection<FleetAirliner>();

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        public ObservableCollection<FleetAirliner> SelectedAirliners { get; set; }

        #endregion

        #region Methods

        private void btnEditAirliners_Click(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>(Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageAirlineEditAirliners(SelectedAirliners.ToList()) { Tag = Tag });
            }
        }

        private void btnRouteMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(Airline.Airline.Routes);
        }
        private void btnCallBackAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((Button)sender).Tag;

            Airline.CallbackAirliner(airliner);

        }
        private void btnMoveAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((Button)sender).Tag;
            
            ComboBox cbAirlines = new ComboBox();
            cbAirlines.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAirlines.ItemTemplate = Application.Current.Resources["AirlineItem"] as DataTemplate;
            cbAirlines.HorizontalAlignment = HorizontalAlignment.Left;
            cbAirlines.Width = 200;

            if (Airline.Airline.Subsidiaries.Count > 0)
                foreach (SubsidiaryAirline sAirline in Airline.Airline.Subsidiaries)
                    cbAirlines.Items.Add(sAirline);
            else
                cbAirlines.Items.Add(((SubsidiaryAirline)Airline.Airline).Airline);

            cbAirlines.SelectedIndex = 0;

            if (
                PopUpSingleElement.ShowPopUp(
                    Translator.GetInstance().GetString("PageAirlineFleet", "1016"),
                    cbAirlines) == PopUpSingleElement.ButtonSelected.OK && cbAirlines.SelectedItem != null)
            {
                var airline = cbAirlines.SelectedItem as Airline;

                Airline.moveAirliner(airliner, airline);
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
                                airliner.RemoveRoute(route);
                            }
                        }

                        Airline.removeAirliner(airliner);

                        AirlineHelpers.AddAirlineInvoice(
                            Airline.Airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.Purchases,
                            airliner.Airliner.GetPrice());

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
                                airliner.RemoveRoute(route);
                            }
                        }

                        Airline.removeAirliner(airliner);

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

            SelectedAirliners.Add(airliner);
        }

        private void cbAirliner_Unchecked(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((CheckBox)sender).Tag;

            SelectedAirliners.Remove(airliner);
        }

        #endregion
    }
}