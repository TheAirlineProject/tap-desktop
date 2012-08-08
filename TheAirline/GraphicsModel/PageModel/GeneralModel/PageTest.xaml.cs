using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.UserControlModel.CalendarModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    /// <summary>
    /// Interaction logic for PageTest.xaml
    /// </summary>
    public partial class PageTest : StandardPage
    {
        
        private TextBox txtName, txtSearch, txtTown, txtGates, txtLat, txtLon, txtIATA, txtDST, txtGMT;
        private ComboBox cbCountry, cbSize, cbType;
        private Button btnSave;
        public PageTest()
        {
            InitializeComponent();

            StackPanel airportPanel = new StackPanel();
            airportPanel.Margin = new Thickness(10, 0, 10, 0);

            // airportPanel.Children.Add(createQuickInfoPanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(airportPanel, StandardContentPanel.ContentLocation.Left);

            Panel panelSideMenu = createSidePanel();

            panelContent.setContentPage(panelSideMenu, StandardContentPanel.ContentLocation.Right);

            StackPanel panelMain = new StackPanel();
            panelContent.Children.Add(panelMain);

            ListBox lbTest = new ListBox();
            lbTest.MaxHeight = 400;
            lbTest.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
    
            foreach (Region region in Regions.GetRegions())
            {

                Expander expRegion = new Expander();
                expRegion.Header = region.Name + ": " + Airports.GetAirports(region).Count;
                expRegion.IsExpanded = false;
                //expRegion.FlowDirection = System.Windows.FlowDirection.RightToLeft;
                expRegion.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                ListBox lbRegion = new ListBox();
                lbRegion.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
                lbRegion.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

                expRegion.Content = lbRegion;

                foreach (Country country in Countries.GetCountries(region))
                {
                    int value = Airports.GetAirports(country).Count;

                    lbRegion.Items.Add(new QuickInfoValue(country.Name, UICreator.CreateTextBlock(value.ToString())));

                }
                lbTest.Items.Add(expRegion);

            }



            lbTest.Items.Add(UICreator.CreateTextBlock(string.Format("Total: {0}", Airports.Count())));

            panelMain.Children.Add(lbTest);

            ListView lvAirports = new ListView();
            lvAirports.Background = Brushes.Transparent;
            lvAirports.SetResourceReference(ListView.ItemContainerStyleProperty, "ListViewItemStyle");
            lvAirports.MaxHeight = 300;
            lvAirports.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler), true);
            lvAirports.BorderThickness = new Thickness(0);
            lvAirports.View = this.Resources["AirportsView"] as GridView;

            //foreach (Airport airport in Airports.GetAirports())
              //  lvAirports.Items.Add(airport);
            lvAirports.ItemsSource = Airports.GetAirports();

            panelMain.Children.Add(lvAirports);

            Button btnDestinations = new Button();
            btnDestinations.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDestinations.Width = Double.NaN;
            btnDestinations.Height = Double.NaN;
            btnDestinations.Content = "Destinations";
            btnDestinations.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDestinations.Click += new RoutedEventHandler(btnDestinations_Click);
            btnDestinations.Margin = new Thickness(0, 10, 0, 0);
            btnDestinations.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelMain.Children.Add(btnDestinations);
          
            base.setContent(panelContent);

            base.setHeaderContent("Test Page");



            showPage(this);

        

            StackPanel sidePanel = new StackPanel();

            /*
            ListBox lbEntry = new ListBox();
            lbEntry.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbEntry.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            foreach (RouteTimeTableEntry entry in table.Entries)
                lbEntry.Items.Add(new QuickInfoValue(entry.Destination.Airport.Profile.IATACode, UICreator.CreateTextBlock(string.Format("{0} {1}", entry.Day, entry.Time))));

            RouteTimeTableEntry current= table.Entries[table.Entries.Count-1];

            RouteTimeTableEntry next = table.getNextEntry(current);

            lbEntry.Items.Add(new QuickInfoValue(current.Destination.Airport.Profile.IATACode, UICreator.CreateTextBlock(string.Format("{0} {1}", current.Day, current.Time))));
            lbEntry.Items.Add(new QuickInfoValue(next.Destination.Airport.Profile.IATACode, UICreator.CreateTextBlock(string.Format("{0} {1}", next.Day, next.Time))));


            sidePanel.Children.Add(lbEntry);
            */

         

        }
        private void GridViewColumnHeaderClickedHandler(object sender,
                                        RoutedEventArgs e)
        {
            
            
            string name = ((GridViewColumnHeader)e.OriginalSource).Content.ToString();

            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(((ListView)sender).ItemsSource);
        
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription("Profile.Name", ListSortDirection.Descending);
            dataView.SortDescriptions.Add(sd);
        }
        private void btnDestinations_Click(object sender, RoutedEventArgs e)
        {
            PopUpMap.ShowPopUp(Airports.GetAirports());
        }
        //creates the side panel
        private Panel createSidePanel()
        {
            StackPanel sidePanel = new StackPanel();

            ListBox lbEntry = new ListBox();
            lbEntry.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbEntry.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            sidePanel.Children.Add(lbEntry);

            WrapPanel panelSearch = new WrapPanel();
      

            txtSearch = new TextBox();
            txtSearch.Foreground = Brushes.White;
            txtSearch.Background = Brushes.Transparent;
            txtSearch.Width = 50;
            txtSearch.MaxLength = 3;

            panelSearch.Children.Add(txtSearch);

            Button btnSearch = new Button();
            btnSearch.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSearch.Height = 16;
            btnSearch.Width = Double.NaN;
            btnSearch.Margin = new Thickness(5, 0, 0, 0);
            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
            btnSearch.Content = "Search";
            btnSearch.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelSearch.Children.Add(btnSearch);
    
            lbEntry.Items.Add(new QuickInfoValue("Search", panelSearch));

            txtName = new TextBox();
            txtName.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("Name", txtName));

            txtIATA = new TextBox();
            txtIATA.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("IATA", txtIATA));

            txtTown = new TextBox();
            txtTown.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("Town", txtTown));

            txtGMT = new TextBox();
            txtGMT.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("GMT", txtGMT));

            txtDST = new TextBox();
            txtDST.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("DST", txtDST));


            cbType = new ComboBox();
            cbType.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbType.Width = 250;

            foreach (AirportProfile.AirportType type in Enum.GetValues(typeof(AirportProfile.AirportType)))
            {
                cbType.Items.Add(type);
            }
            cbType.SelectedIndex = 0;

            lbEntry.Items.Add(new QuickInfoValue("Type", cbType));

            cbCountry = new ComboBox();
            cbCountry.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCountry.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbCountry.Width = 250;

            List<Country> countries = Countries.GetCountries();
            countries.Sort(delegate(Country c1, Country c2) { return c1.Name.CompareTo(c2.Name); });

            foreach (Country country in countries)
                cbCountry.Items.Add(country);

            lbEntry.Items.Add(new QuickInfoValue("Country", cbCountry));

            txtLat = new TextBox();
            txtLat.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("Latitude", txtLat));

            txtLon = new TextBox();
            txtLon.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("Longitude", txtLon));

            txtGates = new TextBox();
            txtGates.Style = this.Resources["TextBoxEditor"] as Style;

            lbEntry.Items.Add(new QuickInfoValue("Gates", txtGates));

            cbSize = new ComboBox();
            cbSize.SetResourceReference(ComboBox.StyleProperty,"ComboBoxTransparentStyle");
            cbSize.Width = 100;

            
     
            foreach (AirportProfile.AirportSize size in Enum.GetValues(typeof(AirportProfile.AirportSize)))
            {
                cbSize.Items.Add(size);
            }
            cbSize.SelectedIndex = 0;

            lbEntry.Items.Add(new QuickInfoValue("Size", cbSize));

            btnSave = new Button();
            btnSave.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSave.Height = 16;
            btnSave.Width = Double.NaN;
            btnSave.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnSave.Content = "Save";
            btnSave.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSave.IsEnabled = false;

            sidePanel.Children.Add(btnSave);
     

            return sidePanel;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iata = txtIATA.Text;
                string name = txtName.Text;
                Country country = (Country)cbCountry.SelectedItem;
                AirportProfile.AirportSize size = (AirportProfile.AirportSize)cbSize.SelectedItem;
                AirportProfile.AirportType type = (AirportProfile.AirportType)cbType.SelectedItem;
                string town = txtTown.Text;
                int gates = Convert.ToInt16(txtGates.Text);

                double gmt = Convert.ToDouble(txtGMT.Text);
                double dst = Convert.ToDouble(txtDST.Text);

                Coordinate latitude = Coordinate.Parse(txtLat.Text);
                Coordinate longitude = Coordinate.Parse(txtLon.Text);



                Airports.AddAirport(new Airport(new AirportProfile(name, iata,"K" + iata, type, town, country, TimeSpan.FromHours(gmt),TimeSpan.FromHours(dst), new Coordinates(latitude, longitude), size,size,Weather.Season.All_Year)));

                saveAirports();

                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1004"), Translator.GetInstance().GetString("MessageBox", "1004", "message"), WPFMessageBoxButtons.Ok);

                btnSave.IsEnabled = false;
            }
            catch (Exception)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1005"), Translator.GetInstance().GetString("MessageBox", "1005", "message"), WPFMessageBoxButtons.Ok);
            }
        }
        //saves the list of airports
        private void saveAirports()
        {
            string path = AppSettings.getDataPath() + "\\airports.xml";

            File.Copy(path, AppSettings.getDataPath() + "\\airports.bak", true);



            XmlDocument xmlDoc = new XmlDocument();

            XmlTextWriter xmlWriter = new XmlTextWriter(path, System.Text.Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("airports");
            xmlWriter.Close();
            xmlDoc.Load(path);

            XmlNode root = xmlDoc.DocumentElement;

            foreach (Airport airport in Airports.GetAirports())
            {
                XmlElement airportNode = xmlDoc.CreateElement("airport");
                airportNode.SetAttribute("name", airport.Profile.Name);
                airportNode.SetAttribute("iata", airport.Profile.IATACode);
                airportNode.SetAttribute("type", airport.Profile.Type.ToString());

                XmlElement townNode = xmlDoc.CreateElement("town");
                townNode.SetAttribute("town", airport.Profile.Town);
                townNode.SetAttribute("country", airport.Profile.Country.Name);
                townNode.SetAttribute("GMT", airport.Profile.OffsetGMT.ToString());
                townNode.SetAttribute("DST", airport.Profile.OffsetDST.ToString());
         
                airportNode.AppendChild(townNode);

                XmlElement coordinatesNode = xmlDoc.CreateElement("coordinates");
                XmlElement latitudeNode = xmlDoc.CreateElement("latitude");
                latitudeNode.SetAttribute("value", airport.Profile.Coordinates.Latitude.ToString());
                XmlElement longitudeNode = xmlDoc.CreateElement("longitude");
                longitudeNode.SetAttribute("value", airport.Profile.Coordinates.Longitude.ToString());
                coordinatesNode.AppendChild(latitudeNode);
                coordinatesNode.AppendChild(longitudeNode);
                airportNode.AppendChild(coordinatesNode);

                XmlElement sizeNode = xmlDoc.CreateElement("size");
                sizeNode.SetAttribute("value", airport.Profile.Size.ToString());
                airportNode.AppendChild(sizeNode);

                XmlElement gatesNode = xmlDoc.CreateElement("gates");
                gatesNode.SetAttribute("value", airport.Profile.ToString());
                airportNode.AppendChild(gatesNode);

                root.AppendChild(airportNode);



            }
            xmlDoc.Save(path);

         
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.LoadFrom(AppSettings.getDataPath() + "\\plugins\\AirportCSVReader.dll");

            Type type = assembly.GetType("AirportCSVReader.CSVReader");

            object instance = Activator.CreateInstance(type, AppSettings.getDataPath() + "\\plugins\\airports.csv");

            string search = txtSearch.Text.Trim();

            object o = type.GetMethod("findAirport").Invoke(instance, new object[] { search });

            if (o != null && Airports.GetAirport(getPropertyValue(o, "IATA").ToString()) == null)
            {
                txtTown.Text = getPropertyValue(o, "Town").ToString();// o.Town;
                txtName.Text = getPropertyValue(o, "Name").ToString();// o.Name;
                txtIATA.Text = getPropertyValue(o, "IATA").ToString();// o.IATA;

                Coordinate latitude = Coordinate.LatitudeToCoordinate(Convert.ToDouble(getPropertyValue(o, "Latitude").ToString()));// o.Latitude));
                Coordinate longitude = Coordinate.LongitudeToCoordinate(Convert.ToDouble(getPropertyValue(o, "Longitude").ToString()));// o.Longitude));

                txtLat.Text = latitude.ToString();
                txtLon.Text = longitude.ToString();

                Country country = Countries.GetCountry(getPropertyValue(o, "Country").ToString());//o.Country);

                if (country != null)
                    cbCountry.SelectedItem = country;

                txtGates.Text = "5";

                btnSave.IsEnabled = true;
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1006"), Translator.GetInstance().GetString("MessageBox", "1006", "message"), WPFMessageBoxButtons.Ok);
        }
                     

        //returns the value of a specific property for a specific type
        private object getPropertyValue(object o, string property)
        {
            return o.GetType().GetProperty(property).GetValue(o, null);
        }
    }
    
    
}
