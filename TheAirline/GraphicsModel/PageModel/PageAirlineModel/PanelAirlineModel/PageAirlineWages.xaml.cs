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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.PageModel.PageAirlineFacilityModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineWages.xaml
    /// </summary>
    public partial class PageAirlineWages : Page
    {
        private Airline Airline;
        private StackPanel panelWages, panelEmployees,panelAirlineServices, panelAdvertisement, panelAirlinePolicies;
        private ScrollViewer panelInflightServices;
        private Dictionary<FeeType, double> FeeValues;
        private ListBox lbWages, lbFees,lbDiscounts, lbFoodDrinks;
        private ListBox lbNewFacilities, lbFacilities, lbAdvertisement;
        private ComboBox cbCancellationPolicy;
        private Dictionary<AdvertisementType.AirlineAdvertisementType, ComboBox> cbAdvertisements;
        private Dictionary<AirlinerClass.ClassType, List<RouteFacility>> Facilities;
        private Dictionary<AirlinerClass.ClassType,List<ComboBox>> cbFacilities;
        public PageAirlineWages(Airline airline)
        {
            InitializeComponent();

            this.Airline = airline;

            this.FeeValues = new Dictionary<FeeType, double>();
      
            FeeTypes.GetTypes().ForEach(f => this.FeeValues.Add(f, this.Airline.Fees.getValue(f)));

            StackPanel panelWagesAndEmployees = new StackPanel();
          
            WrapPanel panelMenuButtons = new WrapPanel();
            panelWagesAndEmployees.Children.Add(panelMenuButtons);

            ucSelectButton sbWages = new ucSelectButton();
            sbWages.Uid = "1004";
            sbWages.Content = Translator.GetInstance().GetString("PageAirlineWages", sbWages.Uid);
            sbWages.IsSelected = true;
            sbWages.Click += new RoutedEventHandler(sbWages_Click);
            panelMenuButtons.Children.Add(sbWages);

            ucSelectButton sbEmployees = new ucSelectButton();
            sbEmployees.Uid = "1005";
            sbEmployees.Content = Translator.GetInstance().GetString("PageAirlineWages", sbEmployees.Uid);
            sbEmployees.Click += new RoutedEventHandler(sbEmployees_Click);
            panelMenuButtons.Children.Add(sbEmployees);

            ucSelectButton sbService = new ucSelectButton();
            sbService.Uid = "1006";
            sbService.Content = Translator.GetInstance().GetString("PageAirlineWages", sbService.Uid);
            sbService.Click += new RoutedEventHandler(sbService_Click);
            panelMenuButtons.Children.Add(sbService);

            ucSelectButton sbAirlineService = new ucSelectButton();
            sbAirlineService.Uid = "1007";
            sbAirlineService.Content = Translator.GetInstance().GetString("PageAirlineWages", sbAirlineService.Uid);
            sbAirlineService.Click += new RoutedEventHandler(sbAirlineService_Click);
            panelMenuButtons.Children.Add(sbAirlineService);

            ucSelectButton sbAdvertisement = new ucSelectButton();
            sbAdvertisement.Uid = "1008";
            sbAdvertisement.Content = Translator.GetInstance().GetString("PageAirlineWages", sbAdvertisement.Uid);
            sbAdvertisement.Click += new RoutedEventHandler(sbAdvertisement_Click);
            panelMenuButtons.Children.Add(sbAdvertisement);

            ucSelectButton sbPolicies = new ucSelectButton();
            sbPolicies.Uid = "1016";
            sbPolicies.Content = Translator.GetInstance().GetString("PageAirlineWages", sbPolicies.Uid);
            sbPolicies.Click += new RoutedEventHandler(sbPolicies_Click);
            panelMenuButtons.Children.Add(sbPolicies);

            panelWages = createWagesPanel();
            panelWagesAndEmployees.Children.Add(panelWages);

            panelEmployees = createEmployeesPanel();
            panelEmployees.Visibility = System.Windows.Visibility.Collapsed;
            panelWagesAndEmployees.Children.Add(panelEmployees);

            panelInflightServices = createInflightServicesPanel();
            panelInflightServices.Visibility = System.Windows.Visibility.Collapsed;
            panelWagesAndEmployees.Children.Add(panelInflightServices);

            panelAirlineServices = createAirlineServicesPanel();
            panelAirlineServices.Visibility = System.Windows.Visibility.Collapsed;
            panelWagesAndEmployees.Children.Add(panelAirlineServices);

            panelAdvertisement = createAdvertisementPanel();
            panelAdvertisement.Visibility = System.Windows.Visibility.Collapsed;
            panelWagesAndEmployees.Children.Add(panelAdvertisement);

            panelAirlinePolicies = createPoliciesPanel();
            panelAirlinePolicies.Visibility = System.Windows.Visibility.Collapsed;
            panelWagesAndEmployees.Children.Add(panelAirlinePolicies);
     
            this.Content = panelWagesAndEmployees;
        }

        
        //creates the for the panel advertisement
        private StackPanel createAdvertisementPanel()
        {
            cbAdvertisements = new Dictionary<AdvertisementType.AirlineAdvertisementType, ComboBox>();

            StackPanel panelAdvertisement = new StackPanel();

            TextBlock txtHeaderAdvertisement = new TextBlock();
            txtHeaderAdvertisement.Uid = "1003";
            txtHeaderAdvertisement.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderAdvertisement.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderAdvertisement.FontWeight = FontWeights.Bold;
            txtHeaderAdvertisement.Text = Translator.GetInstance().GetString("PageAirlineFacilities", txtHeaderAdvertisement.Uid);
            panelAdvertisement.Children.Add(txtHeaderAdvertisement);

            lbAdvertisement = new ListBox();
            lbAdvertisement.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAdvertisement.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;
            lbAdvertisement.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            panelAdvertisement.Children.Add(lbAdvertisement);

            // chs, 2011-17-10 changed so it is only advertisement types which has been invented which are shown
            foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)type)
                    lbAdvertisement.Items.Add(new QuickInfoValue(type.ToString(), createAdvertisementTypeItem(type)));
            }

            Button btnSave = new Button();
            btnSave.Uid = "113";
            btnSave.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSave.Height = 16;
            btnSave.Width = Double.NaN;
            btnSave.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSave.Margin = new Thickness(0, 5, 0, 0);
            btnSave.Content = Translator.GetInstance().GetString("General", btnSave.Uid);
            btnSave.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSave.Click+=new RoutedEventHandler(btnSaveAdvertisement_Click);
            btnSave.Visibility = this.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            
            panelAdvertisement.Children.Add(btnSave);

            return panelAdvertisement;
        }
         //creates the panel for the airline services
        private StackPanel createAirlineServicesPanel()
        {
            StackPanel panelFacilities = new StackPanel();

            TextBlock txtHeaderFacilities = new TextBlock();
            txtHeaderFacilities.Uid = "1001";
            txtHeaderFacilities.Margin = new Thickness(0, 0, 0, 0);
            txtHeaderFacilities.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderFacilities.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderFacilities.FontWeight = FontWeights.Bold;
            txtHeaderFacilities.Text = Translator.GetInstance().GetString("PageAirlineFacilities", txtHeaderFacilities.Uid);
            panelFacilities.Children.Add(txtHeaderFacilities);

            lbFacilities = new ListBox();
            lbFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFacilities.ItemTemplate = this.Resources["FacilityItem"] as DataTemplate;
            lbFacilities.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;
            panelFacilities.Children.Add(lbFacilities);

            TextBlock txtNewAirlineFacilities = new TextBlock();
            txtNewAirlineFacilities.Uid = "1002";
            txtNewAirlineFacilities.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtNewAirlineFacilities.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtNewAirlineFacilities.FontWeight = FontWeights.Bold;
            txtNewAirlineFacilities.Text = Translator.GetInstance().GetString("PageAirlineFacilities", txtNewAirlineFacilities.Uid);
            txtNewAirlineFacilities.Margin = new Thickness(0, 5, 0, 0);
            txtNewAirlineFacilities.Visibility = this.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            panelFacilities.Children.Add(txtNewAirlineFacilities);

            lbNewFacilities = new ListBox();
            lbNewFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbNewFacilities.ItemTemplate = this.Resources["FacilityNewItem"] as DataTemplate;
            lbNewFacilities.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;
            panelFacilities.Children.Add(lbNewFacilities);

            lbNewFacilities.Visibility = this.Airline.IsHuman ? Visibility.Visible : Visibility.Collapsed;

            return panelFacilities;
        }
        //creates the inflight services panel
        private ScrollViewer createInflightServicesPanel()
        {
            this.Facilities = new Dictionary<AirlinerClass.ClassType, List<RouteFacility>>();
            this.cbFacilities = new Dictionary<AirlinerClass.ClassType, List<ComboBox>>();

            ScrollViewer scroller = new ScrollViewer();
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.MaxHeight = GraphicsHelpers.GetContentHeight() - 100;
         

            StackPanel panelServices = new StackPanel();

            /*
            TextBlock txtServicesHeader = new TextBlock();
            txtServicesHeader.Uid = "1007";
            txtServicesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtServicesHeader.FontWeight = FontWeights.Bold;
            txtServicesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtServicesHeader.Text = Translator.GetInstance().GetString("PageAirlineWages", txtServicesHeader.Uid);

            panelServices.Children.Add(txtServicesHeader);
            */
            foreach (AirlinerClass.ClassType classType in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                this.cbFacilities.Add(classType, new List<ComboBox>());

                TextBlock txtClassHeader = new TextBlock();
                txtClassHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                txtClassHeader.FontWeight = FontWeights.Bold;
                txtClassHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
                txtClassHeader.Text = new TextUnderscoreConverter().Convert(classType).ToString();
                txtClassHeader.Margin = new Thickness(0, 5, 0, 0);

                panelServices.Children.Add(txtClassHeader);
    
                ListBox lbServices = new ListBox();
                lbServices.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
                lbServices.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
                
                panelServices.Children.Add(lbServices);

                foreach (RouteFacility.FacilityType facilityType in Enum.GetValues(typeof(RouteFacility.FacilityType)))
                {
                    if (GameObject.GetInstance().GameTime.Year >= (int)facilityType)
                    {
                        ComboBox cbFacility = new ComboBox();
                        cbFacility.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                        cbFacility.Width = 200;
                        cbFacility.DisplayMemberPath = "Name";
                        cbFacility.SelectedValuePath = "Name";
                        cbFacility.Tag = classType;
                        cbFacility.SelectionChanged += new SelectionChangedEventHandler(cbFacility_SelectionChanged);

                        AirlineHelpers.GetRouteFacilities(GameObject.GetInstance().HumanAirline,facilityType).ForEach(f => cbFacility.Items.Add(f));

                        lbServices.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(facilityType).ToString(), cbFacility));

                        cbFacility.SelectedIndex = 0;

                        this.cbFacilities[classType].Add(cbFacility);
                    }
                }

             }

             WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            panelServices.Children.Add(panelButtons);

            Button btnSave = new Button();
            btnSave.Uid = "113";
            btnSave.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSave.Height = Double.NaN;
            btnSave.Width = Double.NaN;
            btnSave.Content = Translator.GetInstance().GetString("General", btnSave.Uid);
            btnSave.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            panelButtons.Children.Add(btnSave);

            Button btnLoad = new Button();
            btnLoad.Uid = "114";
            btnLoad.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLoad.Height = Double.NaN;
            btnLoad.Width = Double.NaN;
            btnLoad.Content = Translator.GetInstance().GetString("General", btnLoad.Uid);
            btnLoad.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnLoad.Click += new RoutedEventHandler(btnLoad_Click);
            panelButtons.Children.Add(btnLoad);


            scroller.Content = panelServices;

            return scroller;
        }
        //creates the employees panel
        private StackPanel createEmployeesPanel()
        {
            StackPanel panelEmployees = new StackPanel();

            TextBlock txtHeaderWages = new TextBlock();
            txtHeaderWages.Uid = "1001";
            txtHeaderWages.Margin = new Thickness(0, 0, 0, 0);
            txtHeaderWages.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderWages.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderWages.FontWeight = FontWeights.Bold;
            txtHeaderWages.Text = Translator.GetInstance().GetString("PageAirlineWages", txtHeaderWages.Uid);

            panelEmployees.Children.Add(txtHeaderWages);

            lbWages = new ListBox();
            lbWages.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbWages.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Wage))
                lbWages.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            panelEmployees.Children.Add(lbWages);

            TextBlock txtEmployeesHeader = new TextBlock();
            txtEmployeesHeader.Uid = "1005";
            txtEmployeesHeader.Margin = new Thickness(0, 5, 0, 0);
            txtEmployeesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtEmployeesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtEmployeesHeader.FontWeight = FontWeights.Bold;
            txtEmployeesHeader.Text = Translator.GetInstance().GetString("PageAirlineWages", txtEmployeesHeader.Uid);

            panelEmployees.Children.Add(txtEmployeesHeader);

            ListBox lbEmployees = new ListBox();
            lbEmployees.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbEmployees.ItemTemplate = this.Resources["EmployeeItem"] as DataTemplate;

            int cockpitCrew = this.Airline.Pilots.Count;//this.Airline.Fleet.Sum(f => f.Airliner.Type.CockpitCrew);

            //var list = (from r in this.Airline.Fleet.SelectMany(f => f.Routes) select r);

            int cabinCrew = this.Airline.Routes.Where(r=>r.Type == Route.RouteType.Passenger).Sum(r => ((PassengerRoute)r).getTotalCabinCrew());

            int serviceCrew = this.Airline.Airports.SelectMany(a=>a.getCurrentAirportFacilities(this.Airline)).Where(a=>a.EmployeeType==AirportFacility.EmployeeTypes.Support).Sum(a=>a.NumberOfEmployees);
            int maintenanceCrew = this.Airline.Airports.SelectMany(a => a.getCurrentAirportFacilities(this.Airline)).Where(a=>a.EmployeeType==AirportFacility.EmployeeTypes.Maintenance).Sum(a => a.NumberOfEmployees);

            lbEmployees.Items.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirlineWages", "1009"), cockpitCrew));
            lbEmployees.Items.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirlineWages", "1010"), cabinCrew));
            lbEmployees.Items.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirlineWages", "1011"), serviceCrew));
            lbEmployees.Items.Add(new KeyValuePair<string, int>(Translator.GetInstance().GetString("PageAirlineWages", "1012"), maintenanceCrew));            

            panelEmployees.Children.Add(lbEmployees);

            panelEmployees.Children.Add(createButtonsPanel());

            return panelEmployees;
        }
        //creates the policies panel
        private StackPanel createPoliciesPanel()
        {
            StackPanel panelPolicies = new StackPanel();

            TextBlock txtHeaderPolicies = new TextBlock();
            txtHeaderPolicies.Uid = "1016";
            //txtHeaderFoods.Margin = new Thickness(0, 5, 0, 0);
            txtHeaderPolicies.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderPolicies.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderPolicies.FontWeight = FontWeights.Bold;
            txtHeaderPolicies.Text = Translator.GetInstance().GetString("PageAirlineWages", txtHeaderPolicies.Uid);

            panelPolicies.Children.Add(txtHeaderPolicies);

            ListBox lbPolicies = new ListBox();
            lbPolicies.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbPolicies.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            panelPolicies.Children.Add(lbPolicies);

            cbCancellationPolicy = new ComboBox();
            cbCancellationPolicy.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCancellationPolicy.Width = 200;
            cbCancellationPolicy.ItemStringFormat = "{0} " + Translator.GetInstance().GetString("PageAirlineWages","1018"); 

            for (int i = 120; i < 300; i += 15)
                cbCancellationPolicy.Items.Add(i);

            cbCancellationPolicy.SelectedItem = this.Airline.getAirlinePolicy("Cancellation Minutes").PolicyValue;

            lbPolicies.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineWages","1017"),cbCancellationPolicy));

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOkCancellation_Click);
            btnOk.Margin = new Thickness(0, 5, 0, 0);
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelPolicies.Children.Add(btnOk);

            return panelPolicies;

        }
        //creates the wage panel
        private StackPanel createWagesPanel()
        {
            StackPanel panelWagesFee = new StackPanel();

            TextBlock txtHeaderFoods = new TextBlock();
            txtHeaderFoods.Uid = "1002";
            //txtHeaderFoods.Margin = new Thickness(0, 5, 0, 0);
            txtHeaderFoods.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderFoods.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderFoods.FontWeight = FontWeights.Bold;
            txtHeaderFoods.Text = Translator.GetInstance().GetString("PageAirlineWages", txtHeaderFoods.Uid);

            panelWagesFee.Children.Add(txtHeaderFoods);

            lbFoodDrinks = new ListBox();
            lbFoodDrinks.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFoodDrinks.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks))
                lbFoodDrinks.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));

            panelWagesFee.Children.Add(lbFoodDrinks);

            TextBlock txtHeaderFees = new TextBlock();
            txtHeaderFees.Uid = "1003";
            txtHeaderFees.Margin = new Thickness(0, 5, 0, 0);
            txtHeaderFees.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderFees.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderFees.FontWeight = FontWeights.Bold;
            txtHeaderFees.Text = Translator.GetInstance().GetString("PageAirlineWages", txtHeaderFees.Uid);

            panelWagesFee.Children.Add(txtHeaderFees);

            lbFees = new ListBox();
            lbFees.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFees.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Fee).FindAll(f=>f.FromYear<=GameObject.GetInstance().GameTime.Year))
                lbFees.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            
            panelWagesFee.Children.Add(lbFees);

            TextBlock txtHeaderDiscounts = new TextBlock();
            txtHeaderDiscounts.Uid = "1015";
            txtHeaderDiscounts.Margin = new Thickness(0, 5, 0, 0);
            txtHeaderDiscounts.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderDiscounts.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderDiscounts.FontWeight = FontWeights.Bold;
            txtHeaderDiscounts.Text = Translator.GetInstance().GetString("PageAirlineWages",txtHeaderDiscounts.Uid);

            panelWagesFee.Children.Add(txtHeaderDiscounts);

            lbDiscounts = new ListBox();
            lbDiscounts.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbDiscounts.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Discount).FindAll(f => f.FromYear <= GameObject.GetInstance().GameTime.Year))
                lbDiscounts.Items.Add(new QuickInfoValue(type.Name, createDiscountSlider(type)));

            panelWagesFee.Children.Add(lbDiscounts);

            panelWagesFee.Children.Add(createButtonsPanel());

            return panelWagesFee;
        }
         private void cbFacility_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             RouteFacility facility = (RouteFacility)((ComboBox)sender).SelectedItem;
             AirlinerClass.ClassType classType = (AirlinerClass.ClassType)((ComboBox)sender).Tag;

             if (!this.Facilities.ContainsKey(classType))
                 this.Facilities.Add(classType, new List<RouteFacility>());
             else
                 if (this.Facilities[classType].Exists(r=>r.Type==facility.Type))
                     this.Facilities[classType].RemoveAll(r=>r.Type==facility.Type);

             this.Facilities[classType].Add(facility);

        }
        private void sbAdvertisement_Click(object sender, RoutedEventArgs e)
         {
             panelWages.Visibility = System.Windows.Visibility.Collapsed;
             panelEmployees.Visibility = System.Windows.Visibility.Collapsed;
             panelInflightServices.Visibility = System.Windows.Visibility.Collapsed;
             panelAirlineServices.Visibility = System.Windows.Visibility.Collapsed;
             panelAdvertisement.Visibility = System.Windows.Visibility.Visible;
             panelAirlinePolicies.Visibility = System.Windows.Visibility.Collapsed;
       
             showAdvertisements();
         }
   
        private void sbAirlineService_Click(object sender, RoutedEventArgs e)
        {
            panelWages.Visibility = System.Windows.Visibility.Collapsed;
            panelEmployees.Visibility = System.Windows.Visibility.Collapsed;
            panelInflightServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlineServices.Visibility = System.Windows.Visibility.Visible;
            panelAdvertisement.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlinePolicies.Visibility = System.Windows.Visibility.Collapsed;
                   
            showFacilities();
     
        }
        private void sbEmployees_Click(object sender, RoutedEventArgs e)
        {
            panelWages.Visibility = System.Windows.Visibility.Collapsed;
            panelEmployees.Visibility = System.Windows.Visibility.Visible;
            panelInflightServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlineServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAdvertisement.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlinePolicies.Visibility = System.Windows.Visibility.Collapsed;
         
            undoSettings();
        }

        private void sbWages_Click(object sender, RoutedEventArgs e)
        {
            panelWages.Visibility = System.Windows.Visibility.Visible;
            panelEmployees.Visibility = System.Windows.Visibility.Collapsed;
            panelInflightServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlineServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAdvertisement.Visibility = System.Windows.Visibility.Collapsed; 
            panelAirlinePolicies.Visibility = System.Windows.Visibility.Collapsed;
       
  
            undoSettings();
        }
        private void sbService_Click(object sender, RoutedEventArgs e)
        {
            panelWages.Visibility = System.Windows.Visibility.Collapsed;
            panelEmployees.Visibility = System.Windows.Visibility.Collapsed;
            panelInflightServices.Visibility = System.Windows.Visibility.Visible;
            panelAirlineServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAdvertisement.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlinePolicies.Visibility = System.Windows.Visibility.Collapsed;
       
           
        }
        private void sbPolicies_Click(object sender, RoutedEventArgs e)
        {
            panelWages.Visibility = System.Windows.Visibility.Collapsed;
            panelEmployees.Visibility = System.Windows.Visibility.Collapsed;
            panelInflightServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlineServices.Visibility = System.Windows.Visibility.Collapsed;
            panelAdvertisement.Visibility = System.Windows.Visibility.Collapsed;
            panelAirlinePolicies.Visibility = System.Windows.Visibility.Visible;

            cbCancellationPolicy.SelectedItem = GameObject.GetInstance().HumanAirline.getAirlinePolicy("Cancellation Minutes").PolicyValue;
   
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100"; 
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnOk);

            Button btnUndo = new Button();
            btnUndo.Uid = "103";
            btnUndo.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnUndo.Height = Double.NaN;
            btnUndo.Margin = new Thickness(5, 0, 0, 0);
            btnUndo.Width = Double.NaN;
            btnUndo.Click += new RoutedEventHandler(btnUndo_Click);
            btnUndo.Content = Translator.GetInstance().GetString("General", btnUndo.Uid);
            btnUndo.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnUndo);

            return buttonsPanel;
        }
        //undos the settings
        private void undoSettings()
        {
            lbWages.Items.Clear();
            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Wage))
            {
                this.FeeValues[type] = this.Airline.Fees.getValue(type);
                lbWages.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            }

            lbFees.Items.Clear();
            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Fee).FindAll(f=>GameObject.GetInstance().GameTime.Year>=f.FromYear))
            {
                
                this.FeeValues[type] = this.Airline.Fees.getValue(type);
                lbFees.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            }
            lbFoodDrinks.Items.Clear();
            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.FoodDrinks))
            {
                this.FeeValues[type] = this.Airline.Fees.getValue(type);
                lbFoodDrinks.Items.Add(new QuickInfoValue(type.Name, createWageSlider(type)));
            }

            lbDiscounts.Items.Clear();
            foreach (FeeType type in FeeTypes.GetTypes(FeeType.eFeeType.Discount))
            {
                this.FeeValues[type] = this.Airline.Fees.getValue(type);
                lbDiscounts.Items.Add(new QuickInfoValue(type.Name, createDiscountSlider(type)));
            }

             
        }
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            undoSettings();
        }
        private void btnOkCancellation_Click(object sender, RoutedEventArgs e)
        {
            int cancellationMinutes = (int)cbCancellationPolicy.SelectedItem;

            GameObject.GetInstance().HumanAirline.setAirlinePolicy("Cancellation Minutes",cancellationMinutes);
          }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2108"), Translator.GetInstance().GetString("MessageBox", "2108", "message"), WPFMessageBoxButtons.YesNo);
            if (result == WPFMessageBoxResult.Yes)
            {
                foreach (FeeType type in this.FeeValues.Keys)
                    this.Airline.Fees.setValue(type, this.FeeValues[type]);
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int totalServiceLevel = this.Facilities.Keys.Sum(c => this.Facilities[c].Sum(f => f.ServiceLevel)) ; 
            TextBox txtName = new TextBox();
            txtName.Width = 200;
            txtName.Background = Brushes.Transparent;
            txtName.Foreground = Brushes.White;
            txtName.Text = string.Format("Configuration {0} (Service level: {1})",Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses).Count+1,totalServiceLevel);
            txtName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirlineWages", "1013"), txtName) == PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                string name = txtName.Text.Trim();
                RouteClassesConfiguration configuration = new RouteClassesConfiguration(name,true);
                foreach (AirlinerClass.ClassType type in this.Facilities.Keys)
                {
                    RouteClassConfiguration classConfiguration = new RouteClassConfiguration(type);

                    foreach (RouteFacility facility in this.Facilities[type])
                    {
                        classConfiguration.addFacility(facility);
                    }
                    configuration.addClass(classConfiguration);
                }

                Configurations.AddConfiguration(configuration);
            }
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
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
                         ComboBox cbFacility = cbFacilities[classConfiguration.Type].Find(cb => ((RouteFacility)cb.SelectedItem).Type == facility.Type);

                         if (GameObject.GetInstance().GameTime.Year >= (int)facility.Type)
                            cbFacility.SelectedItem = facility;
                     }
                 }
            }
        }
        private void btnSaveAdvertisement_Click(object sender, RoutedEventArgs e)
        {
            foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)type)
                {
                    ComboBox cbAdvertisement = cbAdvertisements[type];

                    AdvertisementType aType = (AdvertisementType)cbAdvertisement.SelectedItem;
                    this.Airline.setAirlineAdvertisement(aType);
                }
            }
        }
        //creates the slider for a discount type
        private WrapPanel createDiscountSlider(FeeType type)
        {
            WrapPanel sliderPanel = new WrapPanel();

            TextBlock txtValue = UICreator.CreateTextBlock(string.Format("{0} %", this.FeeValues[type]));
            txtValue.VerticalAlignment = VerticalAlignment.Bottom;
            txtValue.Margin = new Thickness(5, 0, 0, 0);
            txtValue.Tag = type;

            Slider slider = new Slider();
            slider.Width = 200;
            slider.Value = this.FeeValues[type];
            slider.Tag = txtValue;
            slider.Maximum = 100;
            slider.Minimum = type.MinValue;
            slider.ValueChanged +=new RoutedPropertyChangedEventHandler<double>(sliderDiscount_ValueChanged);
            slider.TickFrequency = (100 - type.MinValue) / slider.Width;
            slider.IsSnapToTickEnabled = true;
            slider.IsMoveToPointEnabled = true;
            sliderPanel.Children.Add(slider);

            sliderPanel.Children.Add(txtValue);

            return sliderPanel;
        }
        //creates the slider for a wage type
        private WrapPanel createWageSlider(FeeType type)
        {
            WrapPanel sliderPanel = new WrapPanel();

            TextBlock txtValue = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.FeeValues[type]).ToString());//UICreator.CreateTextBlock(string.Format("{0:C}", this.FeeValues[type]));
            txtValue.VerticalAlignment = VerticalAlignment.Bottom;
            txtValue.Margin = new Thickness(5, 0, 0, 0);
            txtValue.Tag = type;

            Slider slider = new Slider();
            slider.Width = 200;
            slider.Value = this.FeeValues[type];
            slider.Tag = txtValue;
            slider.Maximum = type.MaxValue;
            slider.Minimum = type.MinValue;
            slider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
            slider.TickFrequency = (type.MaxValue - type.MinValue) / slider.Width;
            slider.IsSnapToTickEnabled = true;
            slider.IsMoveToPointEnabled = true;
            sliderPanel.Children.Add(slider);

            sliderPanel.Children.Add(txtValue);

            return sliderPanel;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            TextBlock txtBlock = (TextBlock)slider.Tag;
            txtBlock.Text = new ValueCurrencyConverter().Convert(slider.Value).ToString();// string.Format("{0:C}", slider.Value);

            FeeType type = (FeeType)txtBlock.Tag;

            this.FeeValues[type] = slider.Value;
        }

        private void sliderDiscount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            TextBlock txtBlock = (TextBlock)slider.Tag;
            txtBlock.Text = string.Format("{0} %", slider.Value);

            FeeType type = (FeeType)txtBlock.Tag;

            this.FeeValues[type] = slider.Value;
        }
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AirlineFacility facility = (AirlineFacility)((Button)sender).Tag;

            if (facility.Price > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2101"), Translator.GetInstance().GetString("MessageBox", "2101", "message"), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2102"), string.Format(Translator.GetInstance().GetString("MessageBox", "2102", "message"), facility.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Airline.addFacility(facility);

                    AirlineHelpers.AddAirlineInvoice(this.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -facility.Price);


                    showFacilities();
                }
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            AirlineFacility facility = (AirlineFacility)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2103"), string.Format(Translator.GetInstance().GetString("MessageBox", "2103", "message"), facility.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Airline.removeFacility(facility);

                showFacilities();
            }
        }

        private void lnkAirlineFacility_click(object sender, RoutedEventArgs e)
        {
            AirlineFacility facility = (AirlineFacility)((Hyperlink)sender).Tag;

            if (facility.Shortname == "Maintenance")
            {

                PageNavigator.NavigateTo(new PageAirlineFacilityMaintenance(facility));

            }
        
        }
        //shows the advertisement
        private void showAdvertisements()
        {
            foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)type)
                {
                    ComboBox cbAdvertisement = cbAdvertisements[type];
                    cbAdvertisement.SelectedItem = this.Airline.getAirlineAdvertisement(type);

                }
            }
        }
        //shows the list of facilities
        private void showFacilities()
        {
            int year = GameObject.GetInstance().GameTime.Year;
            lbFacilities.Items.Clear();
            lbNewFacilities.Items.Clear();

            this.Airline.Facilities.ForEach(f => lbFacilities.Items.Add(new KeyValuePair<Airline, AirlineFacility>(this.Airline, f)));

            List<AirlineFacility> facilitiesNew = AirlineFacilities.GetFacilities();

            facilitiesNew.RemoveAll(f => this.Airline.Facilities.Exists(a => a.Uid == f.Uid));
            
            foreach (AirlineFacility facility in facilitiesNew.FindAll(f => f.FromYear <= year))
                lbNewFacilities.Items.Add(facility);
        }
        //creates an item for an advertisering type
        private UIElement createAdvertisementTypeItem(AdvertisementType.AirlineAdvertisementType type)
        {
            if (this.Airline.IsHuman)
            {
                ComboBox cbType = new ComboBox();
                cbType.ItemTemplate = this.Resources["AdvertisementItem"] as DataTemplate;
                cbType.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                cbType.Width = 200;

                cbAdvertisements.Add(type, cbType);

                foreach (AdvertisementType aType in AdvertisementTypes.GetTypes(type))
                    cbType.Items.Add(aType);

                cbType.SelectedItem = this.Airline.getAirlineAdvertisement(type);

                return cbType;
            }
            // chs, 2011-17-10 changed so it is not possible to change the advertisement type for a CPU airline
            else
            {
                return UICreator.CreateTextBlock(this.Airline.getAirlineAdvertisement(type).Name);
            }
        }

      
        
    }

}
