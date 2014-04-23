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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
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
            PilotMVVM pilot = (PilotMVVM)((Button)sender).Tag;

            if (pilot.Pilot.Airliner == null)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2117"), string.Format(Translator.GetInstance().GetString("MessageBox", "2117", "message"), pilot.Pilot.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airline.removePilot(pilot);
                }


            }
            else
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2116"), string.Format(Translator.GetInstance().GetString("MessageBox", "2116", "message"), pilot.Pilot.Profile.Name), WPFMessageBoxButtons.Ok);

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

        private void btnTrainPilot_Click(object sender, RoutedEventArgs e)
        {
            double substituteDayPrice = 500;

            PilotMVVM pilot = (PilotMVVM)((Button)sender).Tag;

            ComboBox cbAirlinerFamily = new ComboBox();
            cbAirlinerFamily.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirlinerFamily.ItemTemplate = this.Resources["TrainingFacility"] as DataTemplate;
            cbAirlinerFamily.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirlinerFamily.Width = 350;

            var airlinerFamilies = AirlinerTypes.GetTypes(t => t.Produced.From.Year < GameObject.GetInstance().GameTime.Year && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30)).Select(t => t.AirlinerFamily).Where(t=>!pilot.Pilot.Aircrafts.Contains(t)).Distinct().OrderBy(a => a);
            
            foreach (string family in airlinerFamilies)
            {
                double price = AirlineHelpers.GetTrainingPrice(pilot.Pilot, family); ;
                cbAirlinerFamily.Items.Add(new PilotTrainingMVVM(family, AirlineHelpers.GetTrainingDays(pilot.Pilot, family), price));
            }

            cbAirlinerFamily.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineEmployees", "1014"), cbAirlinerFamily) == PopUpSingleElement.ButtonSelected.OK && cbAirlinerFamily.SelectedItem != null)
            {
                PilotTrainingMVVM pilotTraining = (PilotTrainingMVVM)cbAirlinerFamily.SelectedItem;

                if (pilot.Pilot.Airliner == null)
                {
                    AirlineHelpers.SendForTraining(GameObject.GetInstance().HumanAirline, pilot.Pilot, pilotTraining.Family, pilotTraining.TrainingDays, pilotTraining.Price);

                    pilot.OnTraining = true;
                }
                else
                {
                    double substitutePrice = GeneralHelpers.GetInflationPrice(pilotTraining.TrainingDays * substituteDayPrice);

                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2129"), string.Format(Translator.GetInstance().GetString("MessageBox", "2129", "message"), new ValueCurrencyConverter().Convert(substitutePrice), pilotTraining.TrainingDays), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        AirlineHelpers.AddAirlineInvoice(this.Airline.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -substitutePrice);
                        AirlineHelpers.SendForTraining(GameObject.GetInstance().HumanAirline, pilot.Pilot, pilotTraining.Family, pilotTraining.TrainingDays, pilotTraining.Price);

                        pilot.OnTraining = true;

                    }

                }
            }


        }
    }
}
