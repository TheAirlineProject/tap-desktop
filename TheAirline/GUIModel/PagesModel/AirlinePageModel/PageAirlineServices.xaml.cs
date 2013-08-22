using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineServices.xaml
    /// </summary>
    public partial class PageAirlineServices : Page
    {
        public AirlineMVVM Airline { get; set; }
        public ObservableCollection<AirlineClassMVVM> Classes { get; set; }
        public List<RouteFacility.FacilityType> FacilityTypes
        {
            get { return this.Classes.SelectMany(c => c.Facilities.Select(f => f.Type)).Distinct().ToList(); }
            private set { ;}
        }
        public PageAirlineServices(AirlineMVVM airline)
        {
            this.Classes = new ObservableCollection<AirlineClassMVVM>();

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                AirlineClassMVVM rClass = new AirlineClassMVVM(type);

                foreach (RouteFacility.FacilityType facilityType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
                {
                    if (GameObject.GetInstance().GameTime.Year >= (int)facilityType)
                    {
                        AirlineClassFacilityMVVM facility = new AirlineClassFacilityMVVM(facilityType);
                        facility.Facilities = AirlineHelpers.GetRouteFacilities(GameObject.GetInstance().HumanAirline, facilityType);

                        rClass.Facilities.Add(facility);
                    }
                }
                this.Classes.Add(rClass);
            }   

            this.Airline = airline;
            this.DataContext = this.Airline;
            this.Loaded += PageAirlineServices_Loaded;
            
            InitializeComponent();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvFacilities.ItemsSource);
            view.GroupDescriptions.Clear();
            view.SortDescriptions.Clear();

            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Type");
            view.GroupDescriptions.Add(groupDescription);

            SortDescription sortTypeDescription = new SortDescription("Type", ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortTypeDescription);

            SortDescription sortFacilityDescription = new SortDescription("Facility.Name",ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortFacilityDescription);

            for (int i = 120; i < 300; i += 15)
                cbCancellationPolicy.Items.Add(i);

            cbCancellationPolicy.SelectedItem = this.Airline.Airline.getAirlinePolicy("Cancellation Minutes").PolicyValue;

        }

        private void PageAirlineServices_Loaded(object sender, RoutedEventArgs e)
        {
            
            foreach (AirlineClassMVVM rClass in this.Classes)
            {
                foreach (AirlineClassFacilityMVVM rFacility in rClass.Facilities)
                {
                    rFacility.SelectedFacility = RouteFacilities.GetBasicFacility(rFacility.Type);//GetFacilities(rFacility.Type).OrderBy(f => f.ServiceLevel).First();

                }

            }
        }

        private void imgSell_Click(object sender, MouseButtonEventArgs e)
        {
            AirlineFacilityMVVM facility = (AirlineFacilityMVVM)((Image)sender).Tag;
          
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2103"), string.Format(Translator.GetInstance().GetString("MessageBox", "2103", "message"), facility.Facility.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airline.removeFacility(facility);
            }

         
            ICollectionView view = CollectionViewSource.GetDefaultView(lvFacilities.ItemsSource);
            view.Refresh();
        }
        private void imgBuy_Click(object sender, MouseButtonEventArgs e)
        {
            AirlineFacilityMVVM facility = (AirlineFacilityMVVM)((Image)sender).Tag;
        
            if (facility.Facility.Price > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2101"), Translator.GetInstance().GetString("MessageBox", "2101", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2102"), string.Format(Translator.GetInstance().GetString("MessageBox", "2102", "message"), facility.Facility.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airline.addFacility(facility);

                }
            }
           
     
            ICollectionView view = CollectionViewSource.GetDefaultView(lvFacilities.ItemsSource);
            view.Refresh();
         
        }
        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            this.Airline.saveFees();

            int cancellationMinutes = (int)cbCancellationPolicy.SelectedItem;

            GameObject.GetInstance().HumanAirline.setAirlinePolicy("Cancellation Minutes", cancellationMinutes);
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            this.Airline.resetFees();

            cbCancellationPolicy.SelectedItem = this.Airline.Airline.getAirlinePolicy("Cancellation Minutes").PolicyValue;
        }

        private void btnSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            int totalServiceLevel = this.Classes.Sum(c => c.Facilities.Sum(f => f.SelectedFacility.ServiceLevel));
            TextBox txtName = new TextBox();
            txtName.Width = 200;
            txtName.Background = Brushes.Transparent;
            txtName.Foreground = Brushes.White;
            txtName.Text = string.Format("Configuration {0} (Service level: {1})", Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses).Count + 1, totalServiceLevel);
            txtName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineWages", "1013"), txtName) == PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                string name = txtName.Text.Trim();
                RouteClassesConfiguration configuration = new RouteClassesConfiguration(name, true);

                foreach (AirlineClassMVVM type in this.Classes)
                {
                    RouteClassConfiguration classConfiguration = new RouteClassConfiguration(type.Type);

                    foreach (AirlineClassFacilityMVVM facility in type.Facilities)
                        classConfiguration.addFacility(facility.SelectedFacility);

                    configuration.addClass(classConfiguration);
                }
               
                Configurations.AddConfiguration(configuration);
            }
        }

        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (RouteClassesConfiguration confItem in Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses))
                cbConfigurations.Items.Add(confItem);

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineWages", "1013"), cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                RouteClassesConfiguration configuration = (RouteClassesConfiguration)cbConfigurations.SelectedItem;

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    foreach (RouteFacility facility in classConfiguration.getFacilities())
                    {

                        AirlineClassMVVM aClass = this.Classes.Where(c => c.Type == classConfiguration.Type).First();
                        aClass.Facilities.Where(f => f.Type == facility.Type).First().SelectedFacility = facility;
                     
                    }
                }
            }
        }
    }
}
