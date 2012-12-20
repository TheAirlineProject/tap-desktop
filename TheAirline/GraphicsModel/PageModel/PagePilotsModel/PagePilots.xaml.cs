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

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel
{
    /// <summary>
    /// Interaction logic for PagePilots.xaml
    /// </summary>
    public partial class PagePilots : StandardPage
    {
        private ListBox lbPilots, lbFlightSchools, lbInstructors;
        private Frame panelSideMenu;
        public PagePilots()
        {
            InitializeComponent();

            this.Uid = "1000";
            this.Title = string.Format(Translator.GetInstance().GetString("PagePilots", this.Uid), GameObject.GetInstance().HumanAirline.Profile.Name);

            StackPanel pilotsPanel = new StackPanel();
            pilotsPanel.Margin = new Thickness(10, 0, 10, 0);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(pilotsPanel, StandardContentPanel.ContentLocation.Left);

            ContentControl txtPilotsHeader = new ContentControl();
            txtPilotsHeader.ContentTemplate = this.Resources["PilotsHeader"] as DataTemplate;
            txtPilotsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            pilotsPanel.Children.Add(txtPilotsHeader);

            lbPilots = new ListBox();
            lbPilots.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPilots.ItemTemplate = this.Resources["PilotItem"] as DataTemplate;
            lbPilots.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100)/3;

            pilotsPanel.Children.Add(lbPilots);

            ContentControl txtInstructorsHeader = new ContentControl();
            txtInstructorsHeader.ContentTemplate = this.Resources["InstructorsHeader"] as DataTemplate;
            txtInstructorsHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtInstructorsHeader.Margin = new Thickness(0, 10, 0, 0);

            pilotsPanel.Children.Add(txtInstructorsHeader);

            lbInstructors = new ListBox();
            lbInstructors.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbInstructors.ItemTemplate = this.Resources["InstructorItem"] as DataTemplate;
            lbInstructors.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;

            pilotsPanel.Children.Add(lbInstructors);

            ContentControl txtFlightSchoolHeader = new ContentControl();
            txtFlightSchoolHeader.ContentTemplate = this.Resources["FlightSchoolsHeader"] as DataTemplate;
            txtFlightSchoolHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtFlightSchoolHeader.Margin = new Thickness(0, 10, 0, 0);

            pilotsPanel.Children.Add(txtFlightSchoolHeader);

            lbFlightSchools = new ListBox();
            lbFlightSchools.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFlightSchools.ItemTemplate = this.Resources["FlightSchoolItem"] as DataTemplate;
            lbFlightSchools.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;

            pilotsPanel.Children.Add(lbFlightSchools);

            panelSideMenu = new Frame();
            
            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);
            
            base.setContent(panelContent);

            base.setHeaderContent(this.Title);

            showPage(this);

            showPilots();

            showFlightSchools();

            showInstructors();

        }
        //shwos the list of instructors
        private void showInstructors()
        {
            lbInstructors.Items.Clear();

            foreach (Instructor instructor in Instructors.GetUnassignedInstructors())
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

            foreach (Pilot pilot in Pilots.GetUnassignedPilots())
            {
                lbPilots.Items.Add(pilot);
            }
        }

        private void lnkPilot_Click(object sender, RoutedEventArgs e)
        {
            Pilot pilot = (Pilot)((Hyperlink)sender).Tag;
            panelSideMenu.Content = new PanelPilot(this,pilot);
          
        }
        public override void updatePage()
        {
            showPilots();
        }
    }
}
