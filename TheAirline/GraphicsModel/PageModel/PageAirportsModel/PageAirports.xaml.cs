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

namespace TheAirline.GraphicsModel.PageModel.PageAirportsModel
{
    /// <summary>
    /// Interaction logic for PageAirports.xaml
    /// </summary>
    public partial class PageAirports : StandardPage
    {
        private ListBox lbAirports;
        private Comparison<Airport> sortCriteria;
        private List<Airport> airportsList;
        public PageAirports()
        {
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

            sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); };

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

      
            airports.Sort(sortCriteria);

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

            switch (type)
            {
                case "Country":
                    sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.Country.Name.CompareTo(a2.Profile.Country.Name); };
                    showAirports();
                    break;
                case "IATA":
                    if (Settings.GetInstance().AirportCodeDisplay == Settings.AirportCode.IATA)
                        sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.IATACode.CompareTo(a2.Profile.IATACode); };
                    else
                        sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.ICAOCode.CompareTo(a2.Profile.ICAOCode); };
              
                    showAirports();
                    break;
                case "Size":
                    sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.Size.CompareTo(a2.Profile.Size); };
                    showAirports();
                    break;
                case "Name":
                    sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); };
                    showAirports();
                    break;
            }
        }
    }
}
