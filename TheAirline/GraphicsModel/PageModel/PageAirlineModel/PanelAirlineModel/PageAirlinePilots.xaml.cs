using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlinePilots.xaml
    /// </summary>
    public partial class PageAirlinePilots : Page
    {
        private Airline Airline;
        private ListBox lbPilots;
        public PageAirlinePilots(Airline airline)
        {
            this.Airline = airline;

            InitializeComponent();

            StackPanel panelPilots = new StackPanel();
            panelPilots.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlinePilots", txtHeader.Uid);
            panelPilots.Children.Add(txtHeader);

            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["PilotsHeader"] as DataTemplate;
            ccHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            panelPilots.Children.Add(ccHeader);

            lbPilots = new ListBox();
            lbPilots.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPilots.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbPilots.ItemTemplate = this.Resources["PilotItem"] as DataTemplate;
            lbPilots.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;
            panelPilots.Children.Add(lbPilots);


            this.Content = panelPilots;

            showPilots();
        }
        //shows the list of pilots
        private void showPilots()
        {
            lbPilots.Items.Clear();

            foreach (Pilot pilot in this.Airline.Pilots)
                lbPilots.Items.Add(pilot);

        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Button)sender).Tag;

            if (pilot.Airliner == null)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2117"), string.Format(Translator.GetInstance().GetString("MessageBox", "2117", "message"),pilot.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    pilot.Airline.removePilot(pilot);

                    showPilots();
                }
                
      
            }
            else
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2116"), string.Format(Translator.GetInstance().GetString("MessageBox", "2116", "message"),pilot.Profile.Name), WPFMessageBoxButtons.Ok);
      
            }
        }
        private void lnk_Pilot(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Hyperlink)sender).Tag;

            if (pilot.Airliner == null)
            {
                ComboBox cbAirliners = new ComboBox();
                cbAirliners.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                cbAirliners.SelectedValuePath = "Name";
                cbAirliners.DisplayMemberPath = "Name";
                cbAirliners.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                cbAirliners.Width = 200;

                foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet.FindAll(f => f.Pilots.Count < f.Airliner.Type.CockpitCrew))
                    cbAirliners.Items.Add(airliner);

                 cbAirliners.SelectedIndex = 0;
                
                if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlinePilots", "1002"), cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
                {
                    FleetAirliner airliner = (FleetAirliner)cbAirliners.SelectedItem;
                    airliner.addPilot(pilot);

                    pilot.Airliner = airliner;

                }
            }
            else
            {
                if (pilot.Airliner.Status != FleetAirliner.AirlinerStatus.Stopped)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2115"), Translator.GetInstance().GetString("MessageBox", "2115", "message"), WPFMessageBoxButtons.Ok);
                }
                else
                {
                    ComboBox cbAirliners = new ComboBox();
                    cbAirliners.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                    cbAirliners.SelectedValuePath = "Name";
                    cbAirliners.DisplayMemberPath = "Name";
                    cbAirliners.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    cbAirliners.Width = 200;

                    foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet.FindAll(f => f.Pilots.Count < f.Airliner.Type.CockpitCrew && f!=pilot.Airliner))
                        cbAirliners.Items.Add(airliner);

                    cbAirliners.SelectedIndex = 0;

                    if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlinePilots", "1002"), cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
                    {
                        pilot.Airliner.removePilot(pilot);

                        FleetAirliner airliner = (FleetAirliner)cbAirliners.SelectedItem;
                        airliner.addPilot(pilot);

                        pilot.Airliner = airliner;

                    }
                }
            }

            showPilots();
        }
    }

}
