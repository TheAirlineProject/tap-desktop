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
using TheAirline.Model.PilotModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel.PanelPilotsModel
{
    /// <summary>
    /// Interaction logic for PanelPilot.xaml
    /// </summary>
    public partial class PanelPilot : Page
    {
        private Pilot Pilot;
        private PagePilots ParentPage;
        public PanelPilot(PagePilots parent, Pilot pilot)
        {
            this.ParentPage = parent;
            this.Pilot = pilot;

            InitializeComponent();

            StackPanel panelPilot = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelPilot", txtHeader.Uid);

            panelPilot.Children.Add(txtHeader);

            ListBox lbPilotInformation = new ListBox();
            lbPilotInformation.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbPilotInformation.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot","1002"), UICreator.CreateTextBlock(this.Pilot.Profile.Name)));
            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1003"), UICreator.CreateTextBlock(this.Pilot.Profile.Birthdate.ToShortDateString())));
            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1004"), UICreator.CreateTownPanel(this.Pilot.Profile.Town)));

            ContentControl lblFlag = new ContentControl();
            lblFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            lblFlag.Content = new CountryCurrentCountryConverter().Convert(this.Pilot.Profile.Town.Country);

            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1005"), lblFlag));

            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1006"), UICreator.CreateTextBlock(this.Pilot.EducationTime.ToShortDateString())));
            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1007"), UICreator.CreateTextBlock(this.Pilot.Rating.ToString())));
           // lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1008"), UICreator.CreateTextBlock(string.Format("{0:C}", ((int)this.Pilot.Rating) * 1000))));

            double pilotBasePrice = GameObject.GetInstance().HumanAirline.Fees.getValue(FeeTypes.GetType("Pilot Base Salary"));//GeneralHelpers.GetInflationPrice(133.53);<

            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1008"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(((int)this.Pilot.Rating) * pilotBasePrice).ToString())));
            
            panelPilot.Children.Add(lbPilotInformation);

            panelPilot.Children.Add(createButtonsPanel());

            this.Content = panelPilot;
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
           
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnHire = new Button();
            btnHire.Uid = "200";
            btnHire.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnHire.Height = Double.NaN;
            btnHire.Width = Double.NaN;
            btnHire.Content = Translator.GetInstance().GetString("PanelPilot", btnHire.Uid);
            btnHire.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnHire.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnHire.Click += new RoutedEventHandler(btnHire_Click);
           
            buttonsPanel.Children.Add(btnHire);

            return buttonsPanel;
        }

        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            Boolean isServingCountry = true;// GameObject.GetInstance().HumanAirline.Airports.Exists(a => a.Profile.Country == this.Pilot.Profile.Town.Country);

            if (isServingCountry)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2801"), Translator.GetInstance().GetString("MessageBox", "2801", "message"), WPFMessageBoxButtons.YesNo);


                if (result == WPFMessageBoxResult.Yes)
                {
                    GameObject.GetInstance().HumanAirline.addPilot(this.Pilot);
                    this.ParentPage.updatePage();

                    this.ParentPage.unloadSideMenu();
                }
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2808"), Translator.GetInstance().GetString("MessageBox", "2808", "message"), WPFMessageBoxButtons.Ok);
        }
    }
}
