using TheAirline.Model.GeneralModel.InvoicesModel;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.PilotModel;

    /// <summary>
    ///     Interaction logic for PageFlightSchools.xaml
    /// </summary>
    public partial class PageFlightSchools : Page
    {
        #region Constructors and Destructors

        public PageFlightSchools()
        {
            this.AllInstructors = new ObservableCollection<Instructor>();
            this.FlightSchools = new ObservableCollection<FlightSchool>();

            Instructors.GetUnassignedInstructors().ForEach(i => this.AllInstructors.Add(i));
            GameObject.GetInstance().HumanAirline.FlightSchools.ForEach(f => this.FlightSchools.Add(f));

            this.Loaded += this.PageFlightSchools_Loaded;
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Instructor> AllInstructors { get; set; }

        public ObservableCollection<FlightSchool> FlightSchools { get; set; }

        #endregion

        #region Methods

        private void PageFlightSchools_Loaded(object sender, RoutedEventArgs e)
        {
            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Flightschool").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;

                matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Pilot").FirstOrDefault();

                matchingItem.Visibility = Visibility.Collapsed;
            }
        }

        private void btnBuild_Click(object sender, RoutedEventArgs e)
        {
            double price = GeneralHelpers.GetInflationPrice(267050);

            var cbAirport = new ComboBox();
            cbAirport.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.Width = 200;
            cbAirport.SelectedValuePath = "Profile.Town.Name";
            cbAirport.DisplayMemberPath = "Profile.Town.Name";
            cbAirport.HorizontalAlignment = HorizontalAlignment.Left;

            List<Airport> homeAirports =
                GameObject.GetInstance()
                    .HumanAirline.Airports.FindAll(
                        a =>
                            a.GetCurrentAirportFacility(
                                GameObject.GetInstance().HumanAirline,
                                AirportFacility.FacilityType.Service).TypeLevel > 0);
            homeAirports.AddRange(GameObject.GetInstance().HumanAirline.Airports.FindAll(a => a.IsHub)); //hubs
            homeAirports = homeAirports.Distinct().ToList();

            foreach (Airport airport in homeAirports)
            {
                if (GameObject.GetInstance().HumanAirline.FlightSchools.Find(f => f.Airport == airport) == null)
                {
                    cbAirport.Items.Add(airport);
                }
            }

            cbAirport.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PagePilots", "1004"), cbAirport)
                == PopUpSingleElement.ButtonSelected.OK && cbAirport.SelectedItem != null)
            {
                var airport = (Airport)cbAirport.SelectedItem;

                var fs = new FlightSchool(airport);

                GameObject.GetInstance().HumanAirline.AddFlightSchool(fs);
                this.FlightSchools.Add(fs);

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -price);

                ICollectionView view = CollectionViewSource.GetDefaultView(this.lvInstructors.ItemsSource);
                view.Refresh();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var fs = (FlightSchool)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2812"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2812", "message"),
                    fs.Airport.Profile.Town.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.RemoveFlightSchool(fs);
                this.FlightSchools.Remove(fs);

                if (GameObject.GetInstance().HumanAirline.FlightSchools.Count > 0)
                {
                    var cbFlightSchools = new ComboBox();
                    cbFlightSchools.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
                    cbFlightSchools.Width = 200;
                    cbFlightSchools.SelectedValuePath = "Name";
                    cbFlightSchools.DisplayMemberPath = "Name";
                    cbFlightSchools.HorizontalAlignment = HorizontalAlignment.Left;

                    foreach (
                        FlightSchool fSchool in
                            GameObject.GetInstance()
                                .HumanAirline.FlightSchools.Where(
                                    f =>
                                        f.NumberOfInstructors + fs.NumberOfInstructors
                                        <= FlightSchool.MaxNumberOfInstructors && f != fs))
                    {
                        cbFlightSchools.Items.Add(fSchool);
                    }

                    cbFlightSchools.SelectedIndex = 0;

                    if (PopUpSingleElement.ShowPopUp(
                        Translator.GetInstance().GetString("PageFlightSchools", "1009"),
                        cbFlightSchools) == PopUpSingleElement.ButtonSelected.OK && cbFlightSchools.SelectedItem != null)
                    {
                        var nFlightSchool = (FlightSchool)cbFlightSchools.SelectedItem;

                        foreach (Instructor instructor in fs.Instructors)
                        {
                            instructor.FlightSchool = nFlightSchool;
                            nFlightSchool.AddInstructor(instructor);
                        }

                        var aircrafts = new List<TrainingAircraft>(fs.TrainingAircrafts);
                        foreach (TrainingAircraft aircraft in aircrafts)
                        {
                            aircraft.FlightSchool = nFlightSchool;
                            nFlightSchool.AddTrainingAircraft(aircraft);
                        }

                        ICollectionView view = CollectionViewSource.GetDefaultView(this.lvFlightSchools.ItemsSource);
                        view.Refresh();
                    }
                    else
                    {
                        foreach (TrainingAircraft aircraft in fs.TrainingAircrafts)
                        {
                            double price = aircraft.Type.Price * 0.75;
                            AirlineHelpers.AddAirlineInvoice(
                                GameObject.GetInstance().HumanAirline,
                                GameObject.GetInstance().GameTime,
                                Invoice.InvoiceType.AirlineExpenses,
                                price);
                        }
                    }
                }
            }
        }

        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            var instructor = (Instructor)((Button)sender).Tag;

            var cbFlightSchools = new ComboBox();
            cbFlightSchools.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbFlightSchools.Width = 200;
            cbFlightSchools.SelectedValuePath = "Name";
            cbFlightSchools.DisplayMemberPath = "Name";
            cbFlightSchools.HorizontalAlignment = HorizontalAlignment.Left;

            foreach (
                FlightSchool fs in
                    GameObject.GetInstance()
                        .HumanAirline.FlightSchools.Where(
                            f => f.NumberOfInstructors < FlightSchool.MaxNumberOfInstructors))
            {
                cbFlightSchools.Items.Add(fs);
            }

            cbFlightSchools.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PanelFlightSchool", "1008"),
                cbFlightSchools) == PopUpSingleElement.ButtonSelected.OK && cbFlightSchools.SelectedItem != null)
            {
                var flightSchool = (FlightSchool)cbFlightSchools.SelectedItem;

                flightSchool.AddInstructor(instructor);
                instructor.FlightSchool = flightSchool;

                this.AllInstructors.Remove(instructor);

                ICollectionView view = CollectionViewSource.GetDefaultView(this.lvFlightSchools.ItemsSource);
                view.Refresh();
            }
        }

        private void lnkFlightSchool_Click(object sender, RoutedEventArgs e)
        {
            var fs = (FlightSchool)((Hyperlink)sender).Tag;

            var tab_main = UIHelpers.FindChild<TabControl>(this.Tag as Page, "tabMenu");

            if (tab_main != null)
            {
                TabItem matchingItem =
                    tab_main.Items.Cast<TabItem>().Where(item => item.Tag.ToString() == "Flightschool").FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = fs.Name;
                matchingItem.Visibility = Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowFlightSchool(fs) { Tag = this.Tag });
            }
        }

        #endregion
    }
}