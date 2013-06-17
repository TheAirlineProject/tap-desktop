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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.PilotModel;
using TheAirline.GraphicsModel.PageModel.PagePilotsModel.PanelPilotsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel
{
    /// <summary>
    /// Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : StandardPage
    {
        private ListBox lbPilots, lbFlightSchools, lbInstructors;
        private Frame panelSideMenu;
        private Button btnBuild;
        public PagePilots()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = string.Format(Translator.GetInstance().GetString("PagePilots", this.Uid), GameObject.GetInstance().HumanAirline.Profile.Name);

            StackPanel pilotsPanel = new StackPanel();
            pilotsPanel.Margin = new Thickness(10, 0, 10, 0);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(pilotsPanel, StandardContentPanel.ContentLocation.Left);

            TextBlock txtPilotsHeader = new TextBlock();
            txtPilotsHeader.Uid = "1001";
            txtPilotsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtPilotsHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtPilotsHeader.FontWeight = FontWeights.Bold;
            txtPilotsHeader.Text = Translator.GetInstance().GetString("PagePilots", txtPilotsHeader.Uid);

            pilotsPanel.Children.Add(txtPilotsHeader);

            ContentControl ccPilotsHeader = new ContentControl();
            ccPilotsHeader.ContentTemplate = this.Resources["PilotsHeader"] as DataTemplate;
            ccPilotsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            pilotsPanel.Children.Add(ccPilotsHeader);

            lbPilots = new ListBox();
            lbPilots.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPilots.ItemTemplate = this.Resources["PilotItem"] as DataTemplate;
            lbPilots.MaxHeight = (GraphicsHelpers.GetContentHeight() - 200) / 3;

            pilotsPanel.Children.Add(lbPilots);

            TextBlock txtInstructorsHeader = new TextBlock();
            txtInstructorsHeader.Uid = "1002";
            txtInstructorsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtInstructorsHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtInstructorsHeader.FontWeight = FontWeights.Bold;
            txtInstructorsHeader.Text = Translator.GetInstance().GetString("PagePilots", txtInstructorsHeader.Uid);
            txtInstructorsHeader.Margin = new Thickness(0, 10, 0, 0);

            pilotsPanel.Children.Add(txtInstructorsHeader);


            ContentControl ccInstructorsHeader = new ContentControl();
            ccInstructorsHeader.ContentTemplate = this.Resources["InstructorsHeader"] as DataTemplate;
            ccInstructorsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            pilotsPanel.Children.Add(ccInstructorsHeader);

            lbInstructors = new ListBox();
            lbInstructors.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbInstructors.ItemTemplate = this.Resources["InstructorItem"] as DataTemplate;
            lbInstructors.MaxHeight = (GraphicsHelpers.GetContentHeight() - 200) / 3;

            pilotsPanel.Children.Add(lbInstructors);

            TextBlock txtFlightSchoolsHeader = new TextBlock();
            txtFlightSchoolsHeader.Uid = "1003";
            txtFlightSchoolsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFlightSchoolsHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtFlightSchoolsHeader.FontWeight = FontWeights.Bold;
            txtFlightSchoolsHeader.Text = Translator.GetInstance().GetString("PagePilots", txtInstructorsHeader.Uid);
            txtFlightSchoolsHeader.Margin = new Thickness(0, 10, 0, 0);

            pilotsPanel.Children.Add(txtFlightSchoolsHeader);

            ContentControl ccFlightSchoolHeader = new ContentControl();
            ccFlightSchoolHeader.ContentTemplate = this.Resources["FlightSchoolsHeader"] as DataTemplate;
            ccFlightSchoolHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            pilotsPanel.Children.Add(ccFlightSchoolHeader);

            lbFlightSchools = new ListBox();
            lbFlightSchools.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlightSchools.ItemTemplate = this.Resources["FlightSchoolItem"] as DataTemplate;
            lbFlightSchools.MaxHeight = (GraphicsHelpers.GetContentHeight() - 200) / 3;

            pilotsPanel.Children.Add(lbFlightSchools);

            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            btnBuild = new Button();
            btnBuild.Uid = "200";
            btnBuild.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnBuild.Height = Double.NaN;
            btnBuild.Width = Double.NaN;
            btnBuild.Content = Translator.GetInstance().GetString("PagePilots", btnBuild.Uid);
            btnBuild.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnBuild.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnBuild.Click += new RoutedEventHandler(btnBuild_Click);
           
            buttonsPanel.Children.Add(btnBuild);

            pilotsPanel.Children.Add(buttonsPanel);

            panelSideMenu = new Frame();

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title);

            showPage(this);

            showPilots();

            showFlightSchools();

            showInstructors();

        }

        private void btnBuild_Click(object sender, RoutedEventArgs e)
        {
            double price = GeneralHelpers.GetInflationPrice(267050);

            ComboBox cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.Width = 200;
            cbAirport.SelectedValuePath = "Profile.Town.Name";
            cbAirport.DisplayMemberPath = "Profile.Town.Name";
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            List<Airport> homeAirports = GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.getCurrentAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.Service).TypeLevel > 0);
            homeAirports.AddRange(GameObject.GetInstance().HumanAirline.Airports.FindAll(a=>a.IsHub)); //hubs
            homeAirports = homeAirports.Distinct().ToList();


            foreach (Airport airport in homeAirports)
            {
                if (GameObject.GetInstance().HumanAirline.FlightSchools.Find(f => f.Airport == airport) == null)
                    cbAirport.Items.Add(airport);
            }

            cbAirport.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PagePilots", "1004"), cbAirport) == PopUpSingleElement.ButtonSelected.OK && cbAirport.SelectedItem != null)
            {
                Airport airport = (Airport)cbAirport.SelectedItem;

                FlightSchool fs = new FlightSchool(airport);

                GameObject.GetInstance().HumanAirline.addFlightSchool(fs);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -price);

                showFlightSchools();

           
            }
        }
        //shwos the list of instructors
        private void showInstructors()
        {
            lbInstructors.Items.Clear();

            foreach (Instructor instructor in Instructors.GetUnassignedInstructors().OrderByDescending(i=>i.Profile.Age))
                lbInstructors.Items.Add(instructor);
        }
        //shows the list of flightschools
        private void showFlightSchools()
        {
            lbFlightSchools.Items.Clear();

            foreach (FlightSchool fs in GameObject.GetInstance().HumanAirline.FlightSchools)
            {
                lbFlightSchools.Items.Add(fs);
            }
        }
        //shows the list of pilots
        private void showPilots()
        {
            lbPilots.Items.Clear();

            foreach (Pilot pilot in Pilots.GetUnassignedPilots().OrderByDescending(p=>p.Profile.Age))
            {
                lbPilots.Items.Add(pilot);
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            FlightSchool fs = (FlightSchool)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2804"), string.Format(Translator.GetInstance().GetString("MessageBox", "2804", "message"), fs.Name), WPFMessageBoxButtons.YesNo);
            if (result == WPFMessageBoxResult.Yes)
            {
                foreach (Instructor instructor in fs.Instructors)
                    instructor.FlightSchool = null;

                GameObject.GetInstance().HumanAirline.removeFlightSchool(fs);

                showFlightSchools();
            }
        }
        private void lnkPilot_Click(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Hyperlink)sender).Tag;
            panelSideMenu.Content = new PanelPilot(this, pilot);

        }
        private void lnkInstructor_Click(object sender, RoutedEventArgs e)
        {
            Instructor instructor = (Instructor)((Hyperlink)sender).Tag;
            panelSideMenu.Content = new PanelInstructor(this, instructor);

        }
        private void lnkFlightSchool_Click(object sender, RoutedEventArgs e)
        {
            FlightSchool fs = (FlightSchool)((Hyperlink)sender).Tag;
            panelSideMenu.Content = new PanelFlightSchool(this, fs);

        }
        public override void updatePage()
        {
            showPilots();

            showInstructors();

            showFlightSchools();
        }
        //unloads the side menu
        public void unloadSideMenu()
        {
            panelSideMenu.Content = null;
        }
    }
}
