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
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportsModel.PanelAirportsModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageAirportsModel
{
    /// <summary>
    /// Interaction logic for PageAirports.xaml
    /// </summary>
    public partial class PageAirports : StandardPage
    {
        private ListBox lbAirports;
        private Func<Airport,object> sortCriteria;
        private Boolean sortDescending;
        private List<Airport> airportsList;
        public PageAirports()
        {
            this.sortDescending = false;
            this.Resources["SettingsClass"] = Settings.GetInstance();

            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PageAirports", this.Uid);

            StackPanel airportsPanel = new StackPanel();
            airportsPanel.Margin = new Thickness(10, 0, 10, 0);


            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["AirportsHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            airportsPanel.Children.Add(txtHeader);

            lbAirports = new ListBox();
            lbAirports.ItemTemplate = this.Resources["AirportItem"] as DataTemplate;
            lbAirports.MaxHeight = GraphicsHelpers.GetContentHeight() - 50;
            lbAirports.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            sortCriteria = a => a.Profile.Name;

            showAirports(Airports.GetAllActiveAirports());

            airportsPanel.Children.Add(lbAirports);

            Button btnResultsMap = new Button();
            btnResultsMap.Uid = "1001";
            btnResultsMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnResultsMap.Width = Double.NaN;
            btnResultsMap.Height = Double.NaN;
            btnResultsMap.Content = Translator.GetInstance().GetString("PageAirports", btnResultsMap.Uid);
            btnResultsMap.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnResultsMap.Margin = new Thickness(0, 10, 0, 0);
            btnResultsMap.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnResultsMap.Click += new RoutedEventHandler(btnResultsMap_Click);

            airportsPanel.Children.Add(btnResultsMap);

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airportsPanel, StandardContentPanel.ContentLocation.Left);

            StackPanel panelSearch = new PanelAirports(this);

            panelContent.setContentPage(panelSearch, StandardContentPanel.ContentLocation.Right);

            base.setContent(panelContent);

            base.setHeaderContent(this.Title);

            showPage(this);
        }

        private void btnResultsMap_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(airportsList);
        }

        //shows a list of airports
        public void showAirports(List<Airport> airports)
        {
            airportsList = airports;

            lbAirports.Items.Clear();

            if (this.sortDescending)
                airports = airports.OrderByDescending(sortCriteria).ThenBy(a=>a.Profile.Name).ToList();
            else
                airports = airports.OrderBy(sortCriteria).ThenBy(a => a.Profile.Name).ToList();
           
            foreach (Airport airport in airports)
                lbAirports.Items.Add(airport);

        }

        public void showAirports()
        {
            showAirports(airportsList);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {
            string type = (string)((Hyperlink)sender).Tag;

            Func<Airport, object> oSort = sortCriteria;

            switch (type)
            {
                case "Country":
                    sortCriteria = a=>a.Profile.Country.Name;

                    if (sortCriteria == oSort)
                        this.sortDescending = !this.sortDescending;

                    showAirports();
                    break;
                case "IATA":
                    if (Settings.GetInstance().AirportCodeDisplay == Settings.AirportCode.IATA)
                        sortCriteria = a => a.Profile.IATACode;
                    else
                        sortCriteria = a => a.Profile.ICAOCode;

                    if (sortCriteria == oSort)
                        this.sortDescending = !this.sortDescending;

                    showAirports();
                    break;
                case "Size":
                    sortCriteria = a => a.Profile.Size;

                    if (sortCriteria == oSort)
                        this.sortDescending = !this.sortDescending;

                    showAirports();
                    break;
                case "Name":
                    sortCriteria = a => a.Profile.Name;

                    if (sortCriteria == oSort)
                        this.sortDescending = !this.sortDescending;

                    showAirports();
                    break;
            }
        }
       
        private void btnRent_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Button)sender).Tag;
            Boolean hasCheckin = airport.getAirportFacility(GameObject.GetInstance().HumanAirline, AirportFacility.FacilityType.CheckIn).TypeLevel > 0;

            object o = PopUpAirportContract.ShowPopUp(airport);

           //WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2222"), string.Format(Translator.GetInstance().GetString("MessageBox", "2222", "message"), airport.Profile.Name), WPFMessageBoxButtons.YesNo);
            
           if (o!=null)
           {
               if (!hasCheckin)
               {
                   AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);

                   airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);
                   AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Purchases, -checkinFacility.Price);

               }

               airport.addAirlineContract((AirportContract)o);
          
               showAirports();
           }

        }
    }
    public class IsHumanAirportConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Airport airport = (Airport)value;


            if (GameObject.GetInstance().HumanAirline.Airports.Contains(airport))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //the converter for renting a gate    
    public class RentingGateVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Collapsed;
            try
            {
                Airport airport = (Airport)value;

                Boolean isEnabled = airport.Terminals.getFreeGates() > 0;


                rv = (Visibility)new BooleanToVisibilityConverter().Convert(isEnabled, null, null, null);

            }
            catch (Exception)
            {

            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
