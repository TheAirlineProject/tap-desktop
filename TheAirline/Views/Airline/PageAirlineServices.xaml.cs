using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Helpers;
using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.Pilots;
using TheAirline.Models.Routes;
using TheAirline.ViewModels.Airline;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    ///     Interaction logic for PageAirlineServices.xaml
    /// </summary>
    public partial class PageAirlineServices : Page
    {
        #region Constructors and Destructors

        public PageAirlineServices(AirlineMVVM airline)
        {
            Classes = new ObservableCollection<AirlineClassMVVM>();

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                if ((int)type <= GameObject.GetInstance().GameTime.Year)
                {
                    var rClass = new AirlineClassMVVM(type);

                    foreach (
                        RouteFacility.FacilityType facilityType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
                    {
                        if (GameObject.GetInstance().GameTime.Year >= (int)facilityType)
                        {
                            var facility = new AirlineClassFacilityMVVM(facilityType);

                            facility.Facilities.Clear();

                            foreach (
                                RouteFacility rFacility in
                                    AirlineHelpers.GetRouteFacilities(
                                        GameObject.GetInstance().HumanAirline,
                                        facilityType))
                            {
                                facility.Facilities.Add(rFacility);
                            }

                            facility.SelectedFacility = RouteFacilities.GetBasicFacility(facility.Type);
                                //GetFacilities(rFacility.Type).OrderBy(f => f.ServiceLevel).First();

                            rClass.Facilities.Add(facility);
                        }
                    }
                    Classes.Add(rClass);
                }
            }

            Airline = airline;
            DataContext = Airline;
            Loaded += PageAirlineServices_Loaded;

            InitializeComponent();

            var view = (CollectionView)CollectionViewSource.GetDefaultView(lvFacilities.ItemsSource);
            view.GroupDescriptions.Clear();
            view.SortDescriptions.Clear();

            var groupDescription = new PropertyGroupDescription("Type");
            view.GroupDescriptions.Add(groupDescription);

            var sortTypeDescription = new SortDescription("Type", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortTypeDescription);

            var sortFacilityDescription = new SortDescription("Facility.Name", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortFacilityDescription);

            for (int i = 120; i < 300; i += 15)
            {
                cbCancellationPolicy.Items.Add(i);
            }

            cbCancellationPolicy.SelectedItem =
                Airline.Airline.GetAirlinePolicy("Cancellation Minutes").PolicyValue;
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        public ObservableCollection<AirlineClassMVVM> Classes { get; set; }

        public List<RouteFacility.FacilityType> FacilityTypes
        {
            get
            {
                return Classes.SelectMany(c => c.Facilities.Select(f => f.Type)).Distinct().ToList();
            }
            private set
            {
                ;
            }
        }

        #endregion

        #region Methods

        private void PageAirlineServices_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (AirlineClassMVVM rClass in Classes)
            {
                foreach (AirlineClassFacilityMVVM rFacility in rClass.Facilities)
                {
                    rFacility.SelectedFacility = RouteFacilities.GetBasicFacility(rFacility.Type);
                        //GetFacilities(rFacility.Type).OrderBy(f => f.ServiceLevel).First();
                }
            }
        }

        //updates the facilities

        private void btnBuyTrainingFacility_Click(object sender, RoutedEventArgs e)
        {
            var facility = (AirlineFacilityMVVM)cbTrainingFacilities.SelectedItem;

            if (facility.Facility.Price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2101"),
                    Translator.GetInstance().GetString("MessageBox", "2101", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2102"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2102", "message"),
                        facility.Facility.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    Airline.addTrainingFacility(facility);
                }
            }
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            var facility = (AirlineFacilityMVVM)((Button)sender).Tag;

            if (facility.Facility.Price > GameObject.GetInstance().HumanAirline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2101"),
                    Translator.GetInstance().GetString("MessageBox", "2101", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2102"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2102", "message"),
                        facility.Facility.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    Airline.addFacility(facility);

                    updateClassFacilities();
                }
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(lvFacilities.ItemsSource);
            view.Refresh();
        }

        private void btnCreateConfiguration_Click(object sender, RoutedEventArgs e)
        {
            foreach (AirlineClassMVVM rClass in Classes)
            {
                foreach (AirlineClassFacilityMVVM rFacility in rClass.Facilities)
                {
                    rFacility.SelectedFacility = RouteFacilities.GetBasicFacility(rFacility.Type);
                        //GetFacilities(rFacility.Type).OrderBy(f => f.ServiceLevel).First();
                }
            }

            btnCreate.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Visible;
        }

        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            var cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (
                RouteClassesConfiguration confItem in
                    Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses))
            {
                cbConfigurations.Items.Add(confItem);
            }

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageAirlineWages", "1013"),
                cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                foreach (AirlineClassMVVM rClass in Classes)
                {
                    foreach (AirlineClassFacilityMVVM rFacility in rClass.Facilities)
                    {
                        rFacility.SelectedFacility = RouteFacilities.GetBasicFacility(rFacility.Type);
                            //GetFacilities(rFacility.Type).OrderBy(f => f.ServiceLevel).First();
                    }
                }

                var configuration = (RouteClassesConfiguration)cbConfigurations.SelectedItem;

                foreach (RouteClassConfiguration classConfiguration in configuration.GetClasses())
                {
                    foreach (RouteFacility facility in classConfiguration.GetFacilities())
                    {
                        AirlineClassMVVM aClass =
                            Classes.Where(c => c.Type == classConfiguration.Type).FirstOrDefault();

                        if (aClass != null)
                        {
                            AirlineClassFacilityMVVM aFacility =
                                aClass.Facilities.Where(f => f.Type == facility.Type).FirstOrDefault();

                            if (aFacility != null)
                            {
                                aFacility.SelectedFacility = facility;
                            }
                        }
                    }
                }

                btnSave.Visibility = Visibility.Visible;
            }
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            Airline.saveFees();

            var cancellationMinutes = (int)cbCancellationPolicy.SelectedItem;

            GameObject.GetInstance().HumanAirline.SetAirlinePolicy("Cancellation Minutes", cancellationMinutes);
        }

        private void btnSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            int totalServiceLevel = Classes.Sum(c => c.Facilities.Sum(f => f.SelectedFacility.ServiceLevel));
            var txtName = new TextBox();
            txtName.Width = 200;
            txtName.Background = Brushes.Transparent;
            txtName.Foreground = Brushes.White;
            txtName.Text = string.Format(
                "Configuration {0} (Service level: {1})",
                Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses).Count + 1,
                totalServiceLevel);
            txtName.HorizontalAlignment = HorizontalAlignment.Left;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineWages", "1013"), txtName)
                == PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                string name = txtName.Text.Trim();
                var configuration = new RouteClassesConfiguration(name, true);

                foreach (AirlineClassMVVM type in Classes)
                {
                    var classConfiguration = new RouteClassConfiguration(type.Type);

                    foreach (AirlineClassFacilityMVVM facility in type.Facilities)
                    {
                        classConfiguration.AddFacility(facility.SelectedFacility);
                    }

                    configuration.AddClass(classConfiguration);
                }

                Configurations.AddConfiguration(configuration);

                btnSave.Visibility = Visibility.Collapsed;
                btnCreate.Visibility = Visibility.Visible;
            }
        }

        private void btnSell_Click(object sender, RoutedEventArgs e)
        {
            var facility = (AirlineFacilityMVVM)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2103"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2103", "message"),
                    facility.Facility.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                if (facility.Facility is PilotTrainingFacility)
                {
                    Airline.removeTrainingFacility(facility);
                }
                else
                {
                    Airline.removeFacility(facility);

                    updateClassFacilities();
                }
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(lvFacilities.ItemsSource);
            view.Refresh();
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            Airline.resetFees();

            cbCancellationPolicy.SelectedItem =
                Airline.Airline.GetAirlinePolicy("Cancellation Minutes").PolicyValue;
        }

        private void updateClassFacilities()
        {
            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                AirlineClassMVVM rClass = Classes.FirstOrDefault(c => c.Type == type);

                if (rClass != null)
                {
                    foreach (
                        RouteFacility.FacilityType facilityType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
                    {
                        if (GameObject.GetInstance().GameTime.Year >= (int)facilityType)
                        {
                            AirlineClassFacilityMVVM facility =
                                rClass.Facilities.FirstOrDefault(c => c.Type == facilityType);

                            if (facility == null)
                            {
                                facility = new AirlineClassFacilityMVVM(facilityType);

                                rClass.Facilities.Add(facility);
                            }

                            facility.Facilities.Clear();

                            foreach (
                                RouteFacility rFacility in
                                    AirlineHelpers.GetRouteFacilities(
                                        GameObject.GetInstance().HumanAirline,
                                        facilityType))
                            {
                                facility.Facilities.Add(rFacility);
                            }

                            facility.SelectedFacility = RouteFacilities.GetBasicFacility(facility.Type);
                        }
                    }
                }
            }
        }

        #endregion
    }
}