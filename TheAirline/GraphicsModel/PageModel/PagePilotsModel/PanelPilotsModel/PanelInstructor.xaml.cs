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
using TheAirline.Model.PilotModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel.PanelPilotsModel
{
    /// <summary>
    /// Interaction logic for PanelInstructor.xaml
    /// </summary>
    public partial class PanelInstructor : Page
    {
        private Instructor Instructor;
        private PagePilots ParentPage;
        public PanelInstructor(PagePilots parent, Instructor instructor)
        {
            this.Instructor = instructor;
            this.ParentPage = parent;

            InitializeComponent();

             StackPanel panelInstructor = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelInstructor", txtHeader.Uid);

            panelInstructor.Children.Add(txtHeader);

            ListBox lbPilotInformation = new ListBox();
            lbPilotInformation.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbPilotInformation.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1002"), UICreator.CreateTextBlock(this.Instructor.Profile.Name)));
            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1003"), UICreator.CreateTextBlock(this.Instructor.Profile.Birthdate.ToShortDateString())));
            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1004"), UICreator.CreateTownPanel(this.Instructor.Profile.Town)));

            ContentControl lblFlag = new ContentControl();
            lblFlag.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            lblFlag.Content = new CountryCurrentCountryConverter().Convert(this.Instructor.Profile.Town.Country);

            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1005"), lblFlag));

            double instructorBasePrice = GameObject.GetInstance().HumanAirline.Fees.getValue(FeeTypes.GetType("Instructor Base Salary"));

            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1007"), UICreator.CreateTextBlock(this.Instructor.Rating.ToString())));
            lbPilotInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelPilot", "1008"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(((int)this.Instructor.Rating) * instructorBasePrice).ToString())));//string.Format("{0:C}", ((int)this.Instructor.Rating) * 2000))));

            panelInstructor.Children.Add(lbPilotInformation);

            if (GameObject.GetInstance().HumanAirline.FlightSchools.Count >0)
                panelInstructor.Children.Add(createButtonsPanel());

            this.Content = panelInstructor;
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
            btnHire.Content = Translator.GetInstance().GetString("PanelInstructor", btnHire.Uid);
            btnHire.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnHire.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnHire.Click += new RoutedEventHandler(btnHire_Click);

            buttonsPanel.Children.Add(btnHire);

            return buttonsPanel;
        }

        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            Boolean isServingCountry = true;// GameObject.GetInstance().HumanAirline.Airports.Exists(a => a.Profile.Country == this.Instructor.Profile.Town.Country);

            if (isServingCountry)
            {
                ComboBox cbFlightSchools = new ComboBox();
                cbFlightSchools.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                cbFlightSchools.Width = 200;
                cbFlightSchools.SelectedValuePath = "Name";
                cbFlightSchools.DisplayMemberPath = "Name";
                cbFlightSchools.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                foreach (FlightSchool fs in GameObject.GetInstance().HumanAirline.FlightSchools.Where(f => f.NumberOfInstructors < FlightSchool.MaxNumberOfInstructors))
                    cbFlightSchools.Items.Add(fs);

                cbFlightSchools.SelectedIndex = 0;

                if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelFlightSchool", "1008"), cbFlightSchools) == PopUpSingleElement.ButtonSelected.OK && cbFlightSchools.SelectedItem != null)
                {
                    FlightSchool flightSchool = (FlightSchool)cbFlightSchools.SelectedItem;

                    flightSchool.addInstructor(this.Instructor);
                    this.Instructor.FlightSchool = flightSchool;

                    this.ParentPage.updatePage();

                    this.ParentPage.unloadSideMenu();
                }
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2808"), Translator.GetInstance().GetString("MessageBox", "2808", "message"), WPFMessageBoxButtons.Ok);
        
        }
    }
}
