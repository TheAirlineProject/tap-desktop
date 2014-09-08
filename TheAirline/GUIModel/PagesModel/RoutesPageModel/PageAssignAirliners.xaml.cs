namespace TheAirline.GUIModel.PagesModel.RoutesPageModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.PassengerModel;
    using TheAirline.Model.PilotModel;

    /// <summary>
    ///     Interaction logic for PageAssignAirliners.xaml
    /// </summary>
    public partial class PageAssignAirliners : Page
    {
        #region Constructors and Destructors

        public PageAssignAirliners()
        {
            this.Contracts = new List<SpecialContractMVVM>();

            foreach (SpecialContract sc in
                GameObject.GetInstance().HumanAirline.SpecialContracts)
            {
                DateTime startdate = sc.Date;
                DateTime enddate = sc.Date; 

                if (sc.Type.IsFixedDate)
                    enddate = sc.Type.Period.To;
                
                this.Contracts.Add(new SpecialContractMVVM(sc,startdate,enddate));
            }

            this.Restrictions =
                FlightRestrictions.GetRestrictions()
                    .FindAll(
                        r =>
                            r.StartDate < GameObject.GetInstance().GameTime
                            && r.EndDate > GameObject.GetInstance().GameTime);

            this.Airliners = new List<FleetAirlinerMVVM>();
            foreach (
                FleetAirliner airliner in
                    GameObject.GetInstance()
                        .HumanAirline.Fleet.FindAll(a => a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime && a.Airliner.Status == Airliner.StatusTypes.Normal))
            {
                this.Airliners.Add(new FleetAirlinerMVVM(airliner));
            }

            this.Loaded += this.PageAssignAirliners_Loaded;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<FleetAirlinerMVVM> Airliners { get; set; }

        public List<FlightRestriction> Restrictions { get; set; }

        public List<SpecialContractMVVM> Contracts { get; set; }

        #endregion

        #region Methods

        private void PageAssignAirliners_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Route").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                TabItem airlinerItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                airlinerItem.Visibility = Visibility.Collapsed;
            }
        }
        private void btnReplaceAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirlinerMVVM)((Button)sender).Tag;

            var cbAirliners = new ComboBox();
            cbAirliners.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAirliners.SelectedValuePath = "Name";
            cbAirliners.DisplayMemberPath = "Name";
            cbAirliners.HorizontalAlignment = HorizontalAlignment.Left;
            cbAirliners.Width = 200;

            double maxDistance = airliner.Airliner.Routes.Max(r=>r.getDistance());

            long requiredRunway = airliner.Airliner.Routes.Select(r => r.Destination1).Min(a => a.getMaxRunwayLength());
            requiredRunway = Math.Min(requiredRunway,airliner.Airliner.Routes.Select(r=>r.Destination2).Min(a=>a.getMaxRunwayLength()));

            List<FleetAirliner> airliners =
                GameObject.GetInstance()
                    .HumanAirline.Fleet.FindAll(
                        a =>
                            a != airliner.Airliner && a.Status == FleetAirliner.AirlinerStatus.Stopped && a.Airliner.Status == Airliner.StatusTypes.Normal && a.Routes.Count == 0
                            && a.Airliner.MinRunwaylength <= requiredRunway && a.Airliner.Range >= maxDistance);

            foreach (FleetAirliner fAirliner in airliners)
            {
                cbAirliners.Items.Add(fAirliner);
            }

            cbAirliners.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAssignAirliners", "1004"),
                cbAirliners) == PopUpSingleElement.ButtonSelected.OK && cbAirliners.SelectedItem != null)
            {
                
                var transferAirliner = (FleetAirliner)cbAirliners.SelectedItem;
                FleetAirlinerMVVM fAirlinerMVVM = this.Airliners.First(a => a.Airliner == transferAirliner);

                foreach (Route route in airliner.Airliner.Routes)
                {
                    foreach (
                        RouteTimeTableEntry entry in
                            route.TimeTable.Entries.FindAll(en => en.Airliner == airliner.Airliner))
                    {
                        entry.Airliner = transferAirliner;

                    }

                    if (!transferAirliner.Routes.Contains(route))
                    {
                        transferAirliner.addRoute(route);
                        fAirlinerMVVM.Routes.Add(route);
                        
              
                    }
                }
                airliner.Airliner.Routes.Clear();
                airliner.HasRoute = false;

                while (airliner.Routes.Count > 0)
                    airliner.Routes.Remove(airliner.Routes[0]);

                int missingPilots =transferAirliner.Airliner.Type.CockpitCrew - transferAirliner.NumberOfPilots;

                List<Pilot> pilots =
                                 Pilots.GetUnassignedPilots(
                                     p =>
                                         p.Profile.Town.Country == airliner.Airliner.Airliner.Airline.Profile.Country
                                         && p.Aircrafts.Contains(airliner.Airliner.Airliner.Type.AirlinerFamily));

                if (pilots.Count == 0)
                {
                    pilots =
                        Pilots.GetUnassignedPilots(
                            p =>
                                p.Profile.Town.Country.Region
                                == airliner.Airliner.Airliner.Airline.Profile.Country.Region
                                && p.Aircrafts.Contains(airliner.Airliner.Airliner.Type.AirlinerFamily));
                }

                while (pilots.Count < missingPilots)
                {
                    GeneralHelpers.CreatePilots(4, airliner.Airliner.Airliner.Type.AirlinerFamily);
                    pilots =
                        Pilots.GetUnassignedPilots(
                            p => p.Aircrafts.Contains(airliner.Airliner.Airliner.Type.AirlinerFamily));
                }


                for (int i = 0; i < missingPilots; i++)
                {
                    pilots[i].Airliner = transferAirliner;
                    transferAirliner.addPilot(pilots[i]);
                }

           
                fAirlinerMVVM.HasRoute = true;
            }

        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirlinerMVVM)((Button)sender).Tag;

            if (airliner.Airliner.Pilots.Count == airliner.Airliner.Airliner.Type.CockpitCrew)
            {
                if (GameObject.GetInstance().DayRoundEnabled)
                {
                    airliner.setStatus(FleetAirliner.AirlinerStatus.On_route);
                }
                else
                {
                    airliner.setStatus(FleetAirliner.AirlinerStatus.To_route_start);
                }
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2507"),
                    string.Format(Translator.GetInstance().GetString("MessageBox", "2507", "message")),
                    WPFMessageBoxButtons.Ok);
            }
        }
        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            SpecialContractMVVM contract = (SpecialContractMVVM)((Button)sender).Tag;

            PopUpShowSpecialContract.ShowPopUp(contract.Contract);
            
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirlinerMVVM)((Button)sender).Tag;

            airliner.setStatus(FleetAirliner.AirlinerStatus.Stopped);
        }
       private void hlAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirlinerMVVM)((Hyperlink)sender).Tag;

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");
            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (airliner.Airliner.NumberOfPilots == airliner.Airliner.Airliner.Type.CockpitCrew)
            {
                if (tab_main != null)
                {
                    TabItem matchingItem =
                        tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Airliner").FirstOrDefault();

                    //matchingItem.IsSelected = true;
                    matchingItem.Header = airliner.Airliner.Name;
                    matchingItem.Visibility = Visibility.Visible;

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
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2505"),
                            string.Format(Translator.GetInstance().GetString("MessageBox", "2505", "message")),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        List<Pilot> unassignedPilots =
                            GameObject.GetInstance().HumanAirline.Pilots.FindAll(p => p.Airliner == null).ToList();

                        for (int i = 0; i < missingPilots; i++)
                        {
                            unassignedPilots[i].Airliner = airliner.Airliner;
                            airliner.Airliner.addPilot(unassignedPilots[i]);
                        }

                        if (tab_main != null)
                        {
                            TabItem matchingItem =
                                tab_main.Items.Cast<TabItem>()
                                    .Where(item => item.Tag.ToString() == "Airliner")
                                    .FirstOrDefault();

                            //matchingItem.IsSelected = true;
                            matchingItem.Header = airliner.Airliner.Name;
                            matchingItem.Visibility = Visibility.Visible;

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
                    var rnd = new Random();
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2506"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2506", "message"),
                                missingPilots),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        while (airliner.Airliner.Airliner.Type.CockpitCrew > airliner.Airliner.NumberOfPilots)
                        {
                            List<Pilot> pilots =
                                Pilots.GetUnassignedPilots(
                                    p =>
                                        p.Profile.Town.Country == airliner.Airliner.Airliner.Airline.Profile.Country
                                        && p.Aircrafts.Contains(airliner.Airliner.Airliner.Type.AirlinerFamily));

                            if (pilots.Count == 0)
                            {
                                pilots =
                                    Pilots.GetUnassignedPilots(
                                        p =>
                                            p.Profile.Town.Country.Region
                                            == airliner.Airliner.Airliner.Airline.Profile.Country.Region
                                            && p.Aircrafts.Contains(airliner.Airliner.Airliner.Type.AirlinerFamily));
                            }

                            if (pilots.Count == 0)
                            {
                                GeneralHelpers.CreatePilots(4, airliner.Airliner.Airliner.Type.AirlinerFamily);
                                pilots =
                                    Pilots.GetUnassignedPilots(
                                        p => p.Aircrafts.Contains(airliner.Airliner.Airliner.Type.AirlinerFamily));
                            }

                            if (pilots.Count < 5)
                            {
                                GeneralHelpers.CreatePilots(50);
                            }

                            Pilot pilot = pilots.First();

                            airliner.Airliner.Airliner.Airline.addPilot(pilot);
                            pilot.Airliner = airliner.Airliner;
                            airliner.Airliner.addPilot(pilot);
                        }

                        if (tab_main != null)
                        {
                            TabItem matchingItem =
                                tab_main.Items.Cast<TabItem>()
                                    .Where(item => item.Tag.ToString() == "Airliner")
                                    .FirstOrDefault();

                            //matchingItem.IsSelected = true;
                            matchingItem.Header = airliner.Airliner.Name;
                            matchingItem.Visibility = Visibility.Visible;

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

        #endregion
    }
}