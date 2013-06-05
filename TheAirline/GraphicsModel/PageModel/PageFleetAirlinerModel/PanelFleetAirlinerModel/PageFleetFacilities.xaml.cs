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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetFacilities.xaml
    /// </summary>
    public partial class PageFleetFacilities : Page
    {
        private FleetAirliner Airliner;
        //private ListBox lbFacilities;
        private StackPanel panelClassFacilities;
        public PageFleetFacilities(FleetAirliner airliner)
        {
            this.Airliner = airliner;

            InitializeComponent();

            StackPanel panelFacilities = new StackPanel();
            panelFacilities.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airliner Facilities";

            panelFacilities.Children.Add(txtHeader);

            panelClassFacilities = new StackPanel();
            panelFacilities.Children.Add(panelClassFacilities);

          
            showFacilities();

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Visibility = this.Airliner.Airliner.Airline.IsHuman ? Visibility.Visible : Visibility.Collapsed;
            
            panelFacilities.Children.Add(panelButtons);
            
            
            Button btnConfiguration = new Button();
            btnConfiguration.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnConfiguration.Height = Double.NaN;
            btnConfiguration.Width = Double.NaN;
            btnConfiguration.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnConfiguration.Click += new RoutedEventHandler(btnConfiguration_Click);
            btnConfiguration.Content = "Configuration";
            btnConfiguration.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnConfiguration.Visibility =  (this.Airliner.Airliner.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger) && (!this.Airliner.HasRoute || this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped) ? Visibility.Visible : Visibility.Collapsed;

            panelButtons.Children.Add(btnConfiguration);

            Button btnLoadConfiguration = new Button();
            btnLoadConfiguration.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLoadConfiguration.Width = Double.NaN;
            btnLoadConfiguration.Height = Double.NaN;
            btnLoadConfiguration.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnLoadConfiguration.Margin = new Thickness(5, 0, 0, 0);
            btnLoadConfiguration.Content = "Load";
            btnLoadConfiguration.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnLoadConfiguration.Visibility = (!this.Airliner.HasRoute || this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped) ? Visibility.Visible : Visibility.Collapsed;
            btnLoadConfiguration.Click += new RoutedEventHandler(btnLoadConfiguration_Click);

            panelButtons.Children.Add(btnLoadConfiguration);

            Button btnSaveConfiguration = new Button();
            btnSaveConfiguration.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSaveConfiguration.Height = Double.NaN;
            btnSaveConfiguration.Width = Double.NaN;
            btnSaveConfiguration.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSaveConfiguration.Margin = new Thickness(5, 0, 0, 0);
            btnSaveConfiguration.Content = "Save";
            btnSaveConfiguration.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSaveConfiguration.Click += new RoutedEventHandler(btnSaveConfiguration_Click);

            panelButtons.Children.Add(btnSaveConfiguration);
            
            this.Content = panelFacilities;
        }

       
        //shows the facilities
        private void showFacilities()
        {
            panelClassFacilities.Children.Clear();

            foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
            {
                TextBlock txtHeader = new TextBlock();
                txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
                txtHeader.FontWeight = FontWeights.Bold;
                txtHeader.Text = string.Format("{0} ({1} seats)",new TextUnderscoreConverter().Convert(aClass.Type, null, null, null),aClass.SeatingCapacity);

                panelClassFacilities.Children.Add(txtHeader);

                foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
                {
                    AirlinerFacility facility = aClass.getFacility(type);

                    ListBox lbFacilities = new ListBox();
                    lbFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
                    lbFacilities.ItemTemplate = this.Resources["FleetFacilityItem"] as DataTemplate;

                    panelClassFacilities.Children.Add(lbFacilities);

                    lbFacilities.Items.Add(new AirlinerFacilityItem(this.Airliner.Airliner.Airline,aClass, facility));
                    
                   
                }
                panelClassFacilities.Children.Add(new Separator());
            
            }

        }
        private void btnLoadConfiguration_Click(object sender, RoutedEventArgs e)
        {
            AirlinerPassengerType airlinerType = ((AirlinerPassengerType)this.Airliner.Airliner.Type);
      
            ComboBox cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (AirlinerConfiguration confItem in Configurations.GetConfigurations(Configuration.ConfigurationType.Airliner).FindAll(c => ((AirlinerConfiguration)c).getNumberOfClasses() <= airlinerType.MaxAirlinerClasses && ((AirlinerConfiguration)c).MinimumSeats <= airlinerType.MaxSeatingCapacity))
                cbConfigurations.Items.Add(confItem);

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp("Select configuration", cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem!=null)
            {

                AirlinerConfiguration configuration = (AirlinerConfiguration)cbConfigurations.SelectedItem ;

                this.Airliner.Airliner.clearAirlinerClasses();

                foreach (AirlinerClassConfiguration aClass in configuration.Classes)
                {
                    AirlinerClass airlinerClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                    airlinerClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                    foreach (AirlinerFacility facility in aClass.getFacilities())
                        airlinerClass.setFacility(this.Airliner.Airliner.Airline,facility);

                    this.Airliner.Airliner.addAirlinerClass(airlinerClass);
                }

                int seatingDiff = airlinerType.MaxSeatingCapacity - configuration.MinimumSeats;

                this.Airliner.Airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).RegularSeatingCapacity += seatingDiff;

                AirlinerFacility seatingFacility = this.Airliner.Airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).getFacility(AirlinerFacility.FacilityType.Seat);

                int extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                this.Airliner.Airliner.getAirlinerClass(AirlinerClass.ClassType.Economy_Class).SeatingCapacity += extraSeats;

                showFacilities();
            }
        }
        private void btnSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            TextBox txtName = new TextBox();
            txtName.Width = 200;
            txtName.Background = Brushes.Transparent;
            txtName.Foreground = Brushes.White;
            txtName.Text = string.Format("{0} ({1} {2})",this.Airliner.Airliner.Type.Name,this.Airliner.Airliner.Classes.Count, this.Airliner.Airliner.Classes.Count == 1 ? "class" : "classes");
            txtName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;


            if (PopUpSingleElement.ShowPopUp("Select configuration name", txtName) == PopUpSingleElement.ButtonSelected.OK && txtName.Text.Trim().Length > 2)
            {
                string name = txtName.Text.Trim();

                AirlinerConfiguration configuration = new AirlinerConfiguration(name,((AirlinerPassengerType)this.Airliner.Airliner.Type).MaxSeatingCapacity,false);

                foreach (AirlinerClass aClass in this.Airliner.Airliner.Classes)
                {
                    AirlinerClassConfiguration classConf = new AirlinerClassConfiguration(aClass.Type, aClass.SeatingCapacity,aClass.RegularSeatingCapacity);

                    foreach (AirlinerFacility classFacility in aClass.getFacilities())
                        classConf.addFacility(classFacility);

                    configuration.addClassConfiguration(classConf);
                }

                Configurations.AddConfiguration(configuration);
            }
           
        }

        private void btnConfiguration_Click(object sender, RoutedEventArgs e)
        {

            List<AirlinerClass> classes = (List<AirlinerClass>)PopUpAirlinerConfiguration.ShowPopUp(this.Airliner.Airliner);

            if (classes != null)
            {
                this.Airliner.Airliner.clearAirlinerClasses();

                foreach (AirlinerClass aClass in classes)
                    this.Airliner.Airliner.addAirlinerClass(aClass);

                showFacilities();

            }
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((this.Airliner.HasRoute && this.Airliner.Status == FleetAirliner.AirlinerStatus.Stopped) || !this.Airliner.HasRoute)
            {
                AirlinerFacilityItem item = (AirlinerFacilityItem)((Button)sender).Tag;
            
                AirlinerFacility facility = (AirlinerFacility)PopUpAirlinerFacility.ShowPopUp(item.AirlinerClass,item.Facility.Type);

                if (facility != null && item.AirlinerClass.getFacility(item.Facility.Type) != facility)
                {
     
                    if (facility.Type == AirlinerFacility.FacilityType.Seat)
                       item.AirlinerClass.SeatingCapacity = Convert.ToInt16(Convert.ToDouble(item.AirlinerClass.RegularSeatingCapacity) / facility.SeatUses); 
       
                    item.AirlinerClass.setFacility(GameObject.GetInstance().HumanAirline,facility);

                    showFacilities();

                }
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2301"), Translator.GetInstance().GetString("MessageBox", "2301", "message"), WPFMessageBoxButtons.Ok);
        }
        //the class for an item in the list
        private class AirlinerFacilityItem
        {
            public Airline Airline { get; set; }
            public AirlinerFacility Facility { get; set; }
            public AirlinerClass AirlinerClass { get; set; }
            public string Image { get { return string.Format("/data/images/{0}.png", this.Facility.Type); } set { ;} }
            public AirlinerFacilityItem(Airline airline, AirlinerClass aClass, AirlinerFacility facility)
            {
                this.Airline = airline;
                this.AirlinerClass = aClass;
                this.Facility = facility;
            }
        }

    }
}
