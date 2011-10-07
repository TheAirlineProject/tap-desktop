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
using TheAirlineV2.Model.AirlineModel;
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirlineV2.Model.GeneralModel;
using System.Windows.Markup;
using System.Globalization;

namespace TheAirlineV2.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFacilities.xaml
    /// </summary>
    public partial class PageAirlineFacilities : Page
    {
        private Airline Airline;
        private ListBox lbNewFacilities, lbFacilities;
        public PageAirlineFacilities(Airline airline)
        {
            this.Language = XmlLanguage.GetLanguage(new CultureInfo(GameObject.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag); 


            InitializeComponent();

            this.Airline = airline;

            InitializeComponent();

            StackPanel panelFacilities = new StackPanel();
            panelFacilities.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airline Facilities";
            panelFacilities.Children.Add(txtHeader);


      
            lbFacilities = new ListBox();
            lbFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbFacilities.ItemTemplate = this.Resources["FacilityItem"] as DataTemplate;
            lbFacilities.MaxHeight = 400;
            panelFacilities.Children.Add(lbFacilities);

         
            TextBlock txtNewAirlineFacilities = new TextBlock();
            txtNewAirlineFacilities.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtNewAirlineFacilities.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtNewAirlineFacilities.FontWeight = FontWeights.Bold;
            txtNewAirlineFacilities.Text = "Purchase Facilities";
            txtNewAirlineFacilities.Margin = new Thickness(0, 5, 0, 0);
            txtNewAirlineFacilities.Visibility = this.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            panelFacilities.Children.Add(txtNewAirlineFacilities);

            lbNewFacilities = new ListBox();
            lbNewFacilities.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbNewFacilities.ItemTemplate = this.Resources["FacilityNewItem"] as DataTemplate;
            lbNewFacilities.MaxHeight = 400;
            panelFacilities.Children.Add(lbNewFacilities);

        
            lbNewFacilities.Visibility = this.Airline.IsHuman ? Visibility.Visible : Visibility.Collapsed;
           
            this.Content = panelFacilities;

            showFacilities();

        }
      
        //shows the list of facilities
        private void showFacilities()
        {
            lbFacilities.Items.Clear();
            lbNewFacilities.Items.Clear();

            foreach (AirlineFacility facility in this.Airline.Facilities)
                lbFacilities.Items.Add(new AirlineFacilityItem(this.Airline,facility));

            List<AirlineFacility> facilitiesNew = AirlineFacilities.GetFacilities();

            facilitiesNew.RemoveAll((delegate(AirlineFacility af) { return this.Airline.Facilities.Contains(af); }));

            foreach (AirlineFacility facility in facilitiesNew)
                lbNewFacilities.Items.Add(facility);


        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AirlineFacility facility = (AirlineFacility)((Button)sender).Tag;

            if (facility.Price > GameObject.GetInstance().HumanAirline.Money)
                WPFMessageBox.Show("Not enough money", "You don't have any money to buy these facilities", WPFMessageBoxButtons.Ok);
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show("Buy facility", string.Format("Are you sure you want to buy {0} as an airline facility?", facility.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    this.Airline.addFacility(facility);

                    //this.Airline.Money -= facility.Price;

                    this.Airline.addInvoice(new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -facility.Price));


                    showFacilities();
                }
            }

        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
          
            AirlineFacility facility = (AirlineFacility)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show("Remove facility", string.Format("Are you sure you want to remove {0} as an airline facility?", facility.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {

                this.Airline.removeFacility(facility);

                showFacilities();
            }


        }
        //the item for a facility for an airline
        private class AirlineFacilityItem
        {
            public Airline Airline { get; set; }
            public AirlineFacility Facility { get; set; }
            public AirlineFacilityItem(Airline airline, AirlineFacility facility)
            {
                this.Airline = airline;
                this.Facility = facility;
            }
        }
    }
}
