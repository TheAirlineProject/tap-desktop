using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.ViewModels.Airline;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    ///     Interaction logic for PageAirlineEmployees.xaml
    /// </summary>
    public partial class PageAirlineEmployees : Page
    {
        #region Constructors and Destructors

        public PageAirlineEmployees(AirlineMVVM airline)
        {
            Airline = airline;
            DataContext = Airline;

            Airline.resetFees();

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        #endregion

        #region Methods

        private void btnFirePilot_Click(object sender, RoutedEventArgs e)
        {
            var pilot = (PilotMVVM)((Button)sender).Tag;

            if (pilot.Pilot.Airliner == null)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2117"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2117", "message"),
                        pilot.Pilot.Profile.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    Airline.removePilot(pilot);
                }
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2116"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2116", "message"),
                        pilot.Pilot.Profile.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    pilot.Pilot.Airliner.Status = FleetAirliner.AirlinerStatus.Stopped;
                    pilot.Pilot.Airliner.RemovePilot(pilot.Pilot);

                    Airline.removePilot(pilot);
                }
            }
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            Airline.saveFees();
        }

        private void btnTrainPilot_Click(object sender, RoutedEventArgs e)
        {
            double substituteDayPrice = 500;

            var pilot = (PilotMVVM)((Button)sender).Tag;

            var cbAirlinerFamily = new ComboBox();
            cbAirlinerFamily.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAirlinerFamily.ItemTemplate = Resources["TrainingFacility"] as DataTemplate;
            cbAirlinerFamily.HorizontalAlignment = HorizontalAlignment.Left;
            cbAirlinerFamily.Width = 350;

            IOrderedEnumerable<string> airlinerFamilies =
                AirlinerTypes.GetTypes(
                    t =>
                        t.Produced.From.Year <= GameObject.GetInstance().GameTime.Year
                        && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30))
                    .Select(t => t.AirlinerFamily)
                    .Where(t => !pilot.Pilot.Aircrafts.Contains(t))
                    .Distinct()
                    .OrderBy(a => a);

            foreach (string family in airlinerFamilies)
            {
                double price = AirlineHelpers.GetTrainingPrice(pilot.Pilot, family);
                ;
                cbAirlinerFamily.Items.Add(
                    new PilotTrainingMVVM(family, AirlineHelpers.GetTrainingDays(pilot.Pilot, family), price));
            }

            cbAirlinerFamily.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineEmployees", "1014"),
                cbAirlinerFamily) == PopUpSingleElement.ButtonSelected.OK && cbAirlinerFamily.SelectedItem != null)
            {
                var pilotTraining = (PilotTrainingMVVM)cbAirlinerFamily.SelectedItem;

                if (pilot.Pilot.Airliner == null)
                {
                    AirlineHelpers.SendForTraining(
                        GameObject.GetInstance().HumanAirline,
                        pilot.Pilot,
                        pilotTraining.Family,
                        pilotTraining.TrainingDays,
                        pilotTraining.Price);

                    pilot.OnTraining = true;
                }
                else
                {
                    double substitutePrice =
                        GeneralHelpers.GetInflationPrice(pilotTraining.TrainingDays * substituteDayPrice);

                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2129"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2129", "message"),
                                new ValueCurrencyConverter().Convert(substitutePrice),
                                pilotTraining.TrainingDays),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        AirlineHelpers.AddAirlineInvoice(
                            Airline.Airline,
                            GameObject.GetInstance().GameTime,
                            Invoice.InvoiceType.AirlineExpenses,
                            -substitutePrice);
                        AirlineHelpers.SendForTraining(
                            GameObject.GetInstance().HumanAirline,
                            pilot.Pilot,
                            pilotTraining.Family,
                            pilotTraining.TrainingDays,
                            pilotTraining.Price);

                        pilot.OnTraining = true;
                    }
                }
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            Airline.resetFees();
        }

        #endregion
    }
}