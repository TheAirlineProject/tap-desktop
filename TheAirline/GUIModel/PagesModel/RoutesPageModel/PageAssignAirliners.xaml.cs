using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.Model.PassengerModel;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    /// <summary>
    /// Interaction logic for PageAssignAirliners.xaml
    /// </summary>
    public partial class PageAssignAirliners : Page
    {
        public List<FlightRestriction> Restrictions { get; set; }
        public List<FleetAirlinerMVVM> Airliners { get; set; }
        public PageAssignAirliners()
        {
            this.Restrictions = FlightRestrictions.GetRestrictions().FindAll(r => r.StartDate < GameObject.GetInstance().GameTime && r.EndDate > GameObject.GetInstance().GameTime);
           
            this.Airliners = new List<FleetAirlinerMVVM>();
            foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet)
                this.Airliners.Add(new FleetAirlinerMVVM(airliner));

            this.Loaded += PageAssignAirliners_Loaded;

            InitializeComponent();
        }

        private void PageAssignAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Route")
       .FirstOrDefault();

                matchingItem.Visibility = System.Windows.Visibility.Collapsed;

                var airlinerItem = tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Airliner")
       .FirstOrDefault();

                airlinerItem.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            FleetAirlinerMVVM airliner = (FleetAirlinerMVVM)((Button)sender).Tag;

            if (airliner.Airliner.Pilots.Count == airliner.Airliner.Airliner.Type.CockpitCrew)
            {
                if (GameObject.GetInstance().DayRoundEnabled)
                    airliner.setStatus(FleetAirliner.AirlinerStatus.On_route);
                else
                    airliner.setStatus(FleetAirliner.AirlinerStatus.To_route_start);

           }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2507"), string.Format(Translator.GetInstance().GetString("MessageBox", "2507", "message")), WPFMessageBoxButtons.Ok);

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            FleetAirlinerMVVM airliner = (FleetAirlinerMVVM)((Button)sender).Tag;

            airliner.setStatus(FleetAirliner.AirlinerStatus.Stopped);
      
        }

        private void hlAirliner_Click(object sender, RoutedEventArgs e)
        {

            FleetAirlinerMVVM airliner = (FleetAirlinerMVVM)((Hyperlink)sender).Tag;

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");
            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (airliner.Airliner.NumberOfPilots == airliner.Airliner.Airliner.Type.CockpitCrew)
            {

                if (tab_main != null)
                {
                    var matchingItem =
         tab_main.Items.Cast<TabItem>()
           .Where(item => item.Tag.ToString() == "Airliner")
           .FirstOrDefault();

                    //matchingItem.IsSelected = true;
                    matchingItem.Header = airliner.Airliner.Name;
                    matchingItem.Visibility = System.Windows.Visibility.Visible;

                    tab_main.SelectedItem = matchingItem;
                }

             
                if (frmContent != null)
                {
                    frmContent.Navigate(new PageRoutePlanner(airliner.Airliner) { Tag = this.Tag });

                }
             
            }
            else
            {
                int missingPilots = airliner.Airliner.Airliner.Type.CockpitCrew - airliner.Airliner.NumberOfPilots;
                if (GameObject.GetInstance().HumanAirline.Pilots.FindAll(p => p.Airliner == null).Count >= missingPilots)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2505"), string.Format(Translator.GetInstance().GetString("MessageBox", "2505", "message")), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        var unassignedPilots = GameObject.GetInstance().HumanAirline.Pilots.FindAll(p => p.Airliner == null).ToList();

                        for (int i = 0; i < missingPilots; i++)
                        {
                            unassignedPilots[i].Airliner = airliner.Airliner;
                            airliner.Airliner.addPilot(unassignedPilots[i]);
                        }

                        
                        if (tab_main != null)
                        {
                            var matchingItem =
                 tab_main.Items.Cast<TabItem>()
                   .Where(item => item.Tag.ToString() == "Airliner")
                   .FirstOrDefault();

                            //matchingItem.IsSelected = true;
                            matchingItem.Header = airliner.Airliner.Name;
                            matchingItem.Visibility = System.Windows.Visibility.Visible;

                            tab_main.SelectedItem = matchingItem;
                        }

                     
                        if (frmContent != null)
                        {
                            frmContent.Navigate(new PageRoutePlanner(airliner.Airliner) { Tag = this.Tag });

                        }
                    }


                }
                else
                {
                    Random rnd = new Random();
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2506"), string.Format(Translator.GetInstance().GetString("MessageBox", "2506", "message"), missingPilots), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        while (airliner.Airliner.Airliner.Type.CockpitCrew > airliner.Airliner.NumberOfPilots)
                        {
                            var pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country == airliner.Airliner.Airliner.Airline.Profile.Country);

                            if (pilots.Count == 0)
                                pilots = Pilots.GetUnassignedPilots(p => p.Profile.Town.Country.Region == airliner.Airliner.Airliner.Airline.Profile.Country.Region);

                            if (pilots.Count == 0)
                                pilots = Pilots.GetUnassignedPilots();

                            Pilot pilot = pilots.First();

                            airliner.Airliner.Airliner.Airline.addPilot(pilot);
                            pilot.Airliner = airliner.Airliner;
                            airliner.Airliner.addPilot(pilot);
                        }

                        if (tab_main != null)
                        {
                            var matchingItem =
                 tab_main.Items.Cast<TabItem>()
                   .Where(item => item.Tag.ToString() == "Airliner")
                   .FirstOrDefault();

                            //matchingItem.IsSelected = true;
                            matchingItem.Header = airliner.Airliner.Name;
                            matchingItem.Visibility = System.Windows.Visibility.Visible;

                            tab_main.SelectedItem = matchingItem;
                        }

                   
                        if (frmContent != null)
                        {
                            frmContent.Navigate(new PageRoutePlanner(airliner.Airliner) { Tag = this.Tag });

                        }


                    }
                }
            }
        }
    }
}
