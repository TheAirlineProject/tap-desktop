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
            InitializeComponent();

            StackPanel airportsPanel = new StackPanel();
            airportsPanel.Margin = new Thickness(10, 0, 10, 0);

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["AirportsHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
           // txtHeader.SetResourceReference(Label.BackgroundProperty, "HeaderBackgroundBrush");

            airportsPanel.Children.Add(txtHeader);
       

            lbAirports = new ListBox();
            lbAirports.ItemTemplate = this.Resources["AirportItem"] as DataTemplate;
            lbAirports.MaxHeight = 500;
            lbAirports.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); };

           
            showAirports(Airports.GetAirports());

            airportsPanel.Children.Add(lbAirports);

            Button btnResultsMap = new Button();
            btnResultsMap.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnResultsMap.Width = 150;
            btnResultsMap.Height = 20;
            btnResultsMap.Content = "Show results on map";
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

            base.setHeaderContent("Airports");

            //base.setHeaderContent(string.Format("{0} Finals", this.League.Profile.ShortName), @"/Data/images/trophy.png");


            //base.setActionMenu(new ActionMenuModel.ActionMenu());

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

           // PageNavigator.NavigateTo(new PagePlayerProfile(player));
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
                    sortCriteria = delegate(Airport a1, Airport a2) { return a1.Profile.IATACode.CompareTo(a2.Profile.IATACode); };
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
