using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFacilities.xaml
    /// </summary>
    public partial class PageAirlineFacilities : Page
    {
        private Airline Airline;
        private ListBox lbNewFacilities, lbFacilities, lbAdvertisement;
        private Dictionary<AdvertisementType.AirlineAdvertisementType, ComboBox> cbAdvertisements;
        public PageAirlineFacilities(Airline airline)
        {
            InitializeComponent();

            this.Airline = airline;

            cbAdvertisements = new Dictionary<AdvertisementType.AirlineAdvertisementType, ComboBox>();

            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag); 

            StackPanel panelFacilities = new StackPanel();
            panelFacilities.Margin = new Thickness(0, 10, 50, 0);

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
            lbFacilities.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;
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
            lbNewFacilities.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;
            panelFacilities.Children.Add(lbNewFacilities);

            lbNewFacilities.Visibility = this.Airline.IsHuman ? Visibility.Visible : Visibility.Collapsed;

            TextBlock txtHeaderAdvertisement = new TextBlock();
            txtHeaderAdvertisement.Uid = "1003";
            txtHeaderAdvertisement.Margin = new Thickness(0, 5, 0, 0);
            txtHeaderAdvertisement.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeaderAdvertisement.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeaderAdvertisement.FontWeight = FontWeights.Bold;
            txtHeaderAdvertisement.Text = Translator.GetInstance().GetString("PageAirlineFacilities", txtHeaderAdvertisement.Uid);
            panelFacilities.Children.Add(txtHeaderAdvertisement);

            lbAdvertisement = new ListBox();
            lbAdvertisement.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAdvertisement.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;
            lbAdvertisement.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            panelFacilities.Children.Add(lbAdvertisement);

            // chs, 2011-17-10 changed so it is only advertisement types which has been invented which are shown
            foreach (AdvertisementType.AirlineAdvertisementType type in Enum.GetValues(typeof(AdvertisementType.AirlineAdvertisementType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)type)
                    lbAdvertisement.Items.Add(new QuickInfoValue(type.ToString(), createAdvertisementTypeItem(type)));
            }

            Button btnSave = new Button();
            btnSave.Uid = "1004";
            btnSave.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSave.Height = 16;
            btnSave.Width = Double.NaN;
            btnSave.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSave.Margin = new Thickness(0, 5, 0, 0);
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnSave.Content = Translator.GetInstance().GetString("PageAirlineFacilities", btnSave.Uid);
            btnSave.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSave.Visibility = this.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            panelFacilities.Children.Add(btnSave);

            this.Content = panelFacilities;

            showFacilities();

        }

        // chs, 2011-14-10 sets the airline advertisement items
        private void btnSave_Click(object sender, RoutedEventArgs e)
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

        //shows the list of facilities
        private void showFacilities()
        {
            int year = GameObject.GetInstance().GameTime.Year;
            lbFacilities.Items.Clear();
            lbNewFacilities.Items.Clear();

            this.Airline.Facilities.ForEach(f => lbFacilities.Items.Add(new KeyValuePair<Airline, AirlineFacility>(this.Airline, f)));
         
            List<AirlineFacility> facilitiesNew = AirlineFacilities.GetFacilities();

            facilitiesNew.RemoveAll(f => this.Airline.Facilities.Contains(f));
           
            foreach (AirlineFacility facility in facilitiesNew.FindAll(f=>f.FromYear<=year)) 
                lbNewFacilities.Items.Add(facility);
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

                    AirlineHelpers.AddAirlineInvoice(this.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -GeneralHelpers.GetInflationPrice(facility.Price));

         
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
        
      
        
    }
}
