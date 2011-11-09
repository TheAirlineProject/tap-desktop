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

            Button btnConfiguration = new Button();
            btnConfiguration.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnConfiguration.Height = Double.NaN;
            btnConfiguration.Width = Double.NaN;
            btnConfiguration.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnConfiguration.Click += new RoutedEventHandler(btnConfiguration_Click);
            btnConfiguration.Content = "Configuration";
            btnConfiguration.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnConfiguration.Visibility = this.Airliner.Airline.IsHuman && (this.Airliner.RouteAirliner == null || this.Airliner.RouteAirliner.Status == RouteAirliner.AirlinerStatus.Stopped) ? Visibility.Visible : Visibility.Collapsed;

            panelFacilities.Children.Add(btnConfiguration);
            //panelFacilities.Children.Add(lbFacilities);

            this.Content = panelFacilities;
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

                    lbFacilities.Items.Add(new AirlinerFacilityItem(this.Airliner.Airline,aClass, facility));
                    
                   
                }
                panelClassFacilities.Children.Add(new Separator());
            
            }

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((this.Airliner.HasRoute && this.Airliner.RouteAirliner.Status == RouteAirliner.AirlinerStatus.Stopped) || !this.Airliner.HasRoute)
            {
                AirlinerFacilityItem item = (AirlinerFacilityItem)((Button)sender).Tag;
            
                AirlinerFacility facility = (AirlinerFacility)PopUpAirlinerFacility.ShowPopUp(item.AirlinerClass,item.Facility.Type);

                if (facility != null && item.AirlinerClass.getFacility(item.Facility.Type) != facility)
                {
     
                    if (facility.Type == AirlinerFacility.FacilityType.Seat)
                       item.AirlinerClass.SeatingCapacity = Convert.ToInt16(Convert.ToDouble(item.AirlinerClass.RegularSeatingCapacity) / facility.SeatUses); 
       
                    item.AirlinerClass.setFacility(facility);

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
