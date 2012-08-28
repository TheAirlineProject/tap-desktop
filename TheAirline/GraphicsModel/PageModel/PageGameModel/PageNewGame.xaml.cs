using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.PassengerModel;

namespace TheAirline.GraphicsModel.PageModel.PageGameModel
{
    /// <summary>
    /// Interaction logic for PageStartNewGame.xaml
    /// </summary>
    public partial class PageNewGame : StandardPage
    {
        private TextBox txtName;
        private TextBlock txtIATA;
        private ContentControl cntCountry;
        private ComboBox cbAirport, cbAirline, cbOpponents, cbStartYear, cbTimeZone;
        private ICollectionView airportsView;
        private Rectangle airlineColorRect;
        public PageNewGame()
        {
            InitializeComponent();

            StackPanel panelContent = new StackPanel();
            panelContent.Margin = new Thickness(10, 0, 10, 0);
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            Panel panelLogo = UICreator.CreateGameLogo();
            panelLogo.Margin = new Thickness(0, 0, 0, 20);

            panelContent.Children.Add(panelLogo);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Uid = "1001";
            txtHeader.Text = Translator.GetInstance().GetString("PageNewGame", txtHeader.Uid);
            panelContent.Children.Add(txtHeader);


            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelContent.Children.Add(lbContent);

            // chs, 2011-19-10 added for the possibility of creating a new airline
            WrapPanel panelAirline = new WrapPanel();

            cbAirline = new ComboBox();
            cbAirline.SetResourceReference(ComboBox.ItemTemplateProperty, "AirlineLogoItem");
            cbAirline.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirline.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirline.SelectionChanged += new SelectionChangedEventHandler(cbAirline_SelectionChanged);
            cbAirline.Width = 200;

            List<Airline> airlines = Airlines.GetAllAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            cbAirline.ItemsSource = airlines;

            panelAirline.Children.Add(cbAirline);

            Button btnAddAirline = new Button();
            btnAddAirline.Margin = new Thickness(5, 0, 0, 0);
            btnAddAirline.Background = Brushes.Transparent;
            btnAddAirline.Click += new RoutedEventHandler(btnAddAirline_Click);

            Image imgAddAirline = new Image();
            imgAddAirline.Source = new BitmapImage(new Uri(@"/Data/images/add.png", UriKind.RelativeOrAbsolute));
            imgAddAirline.Height = 16;
            RenderOptions.SetBitmapScalingMode(imgAddAirline, BitmapScalingMode.HighQuality);

            btnAddAirline.Content = imgAddAirline;

            panelAirline.Children.Add(btnAddAirline);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1002"), panelAirline));

            txtIATA = UICreator.CreateTextBlock("");
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1003"), txtIATA));

            cntCountry = new ContentControl();
            cntCountry.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1004"), cntCountry));


            txtName = new TextBox();
            txtName.Background = Brushes.Transparent;
            txtName.BorderBrush = Brushes.Black;
            txtName.Width = 200;


            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1005"), txtName));

            // chs, 2011-19-10 added to show the airline color
            airlineColorRect = new Rectangle();
            airlineColorRect.Width = 40;
            airlineColorRect.Height = 20;
            airlineColorRect.StrokeThickness = 1;
            airlineColorRect.Stroke = Brushes.Black;
            airlineColorRect.Fill = new AirlineBrushConverter().Convert(Airlines.GetAirline("ZA")) as Brush;
            airlineColorRect.Margin = new Thickness(0, 2, 0, 2);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1006"), airlineColorRect));

            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.IsSynchronizedWithCurrentItem = true;
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirport.SelectionChanged += new SelectionChangedEventHandler(cbAirports_SelectionChanged);

            List<Airport> airportsList = Airports.GetAllAirports();

            airportsView = CollectionViewSource.GetDefaultView(airportsList);
            airportsView.SortDescriptions.Add(new SortDescription("Profile.Name", ListSortDirection.Ascending));

            cbAirport.ItemsSource = airportsView;


            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1007"), cbAirport));


            cbStartYear = new ComboBox();
            cbStartYear.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbStartYear.Width = 60;
            for (int i = 1960; i < DateTime.Now.Year + 2; i++)
                cbStartYear.Items.Add(i);

            cbStartYear.SelectedItem = DateTime.Now.Year;
            cbStartYear.SelectionChanged += new SelectionChangedEventHandler(cbStartYear_SelectionChanged);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1008"), cbStartYear));

            cbTimeZone = new ComboBox();
            cbTimeZone.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbTimeZone.Width = 300;
            cbTimeZone.DisplayMemberPath = "DisplayName";
            cbTimeZone.SelectedValuePath = "DisplayName";


            foreach (GameTimeZone gtz in TimeZones.GetTimeZones())
                cbTimeZone.Items.Add(gtz);

            cbTimeZone.SelectedItem = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == new TimeSpan(0, 0, 0); });

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1009"), cbTimeZone));



            cbOpponents = new ComboBox();
            cbOpponents.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbOpponents.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbOpponents.Width = 50;
            cbOpponents.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            for (int i = 0; i < Airlines.GetAllAirlines().Count; i++)
                cbOpponents.Items.Add(i);

            cbOpponents.SelectedIndex = 3;// cbOpponents.Items.Count - 1;

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1010"), cbOpponents));


            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelContent.Children.Add(panelButtons);


            Button btnCreate = new Button();
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Uid = "201";
            btnCreate.Content = Translator.GetInstance().GetString("PageNewGame", btnCreate.Uid);
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnCreate);

            Button btnExit = new Button();
            btnExit.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnExit.Height = double.NaN;
            btnExit.Width = double.NaN;
            btnExit.Uid = "202";
            btnExit.Content = Translator.GetInstance().GetString("PageNewGame", btnExit.Uid);
            btnExit.Margin = new Thickness(5, 0, 0, 0);
            btnExit.Click += new RoutedEventHandler(btnCancel_Click);
            btnExit.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnExit);

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent(Translator.GetInstance().GetString("PageNewGame", "200"));

            cbAirline.SelectedIndex = 0;


            showPage(this);

        }

        private void cbStartYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Airline airline = (Airline)cbAirline.SelectedItem;
            int year = (int)cbStartYear.SelectedItem;

            setAirportsView(year, airline.Profile.Country);
           
        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            Airline airline = (Airline)cbAirline.SelectedItem;
            int year = (int)cbStartYear.SelectedItem;


            setAirportsView(year, airline.Profile.Country);

            if (airline.Profile.PreferedAirport != null)
                cbAirport.SelectedItem = airline.Profile.PreferedAirport;

            airlineColorRect.Fill = new AirlineBrushConverter().Convert(airline) as Brush;
            txtName.Text = airline.Profile.CEO;
            txtIATA.Text = airline.Profile.IATACode;
            cntCountry.Content = airline.Profile.Country;

        }
        private void btnAddAirline_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewAirline());

        }



        private void cbAirports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTimeZone != null)
            {
                Airport airport = (Airport)cbAirport.SelectedItem;
                cbTimeZone.SelectedItem = airport.Profile.TimeZone;
            }


        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageFrontMenu());

        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text.Trim().Length > 2)
            {
                GameTimeZone gtz = (GameTimeZone)cbTimeZone.SelectedItem;
                GameObject.GetInstance().TimeZone = gtz;

                int startYear = (int)cbStartYear.SelectedItem;
                GameObject.GetInstance().GameTime = new DateTime(startYear, 1, 1);

                //sets the fuel price
                GameObject.GetInstance().FuelPrice = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice;

                int opponents = (int)cbOpponents.SelectedItem;
                Airline airline = (Airline)cbAirline.SelectedItem;

                airline.Profile.CEO = txtName.Text.Trim();

                GameObject.GetInstance().HumanAirline = airline;

                Airport airport = (Airport)cbAirport.SelectedItem;

                airport.Terminals.rentGate(airline); 
                airport.Terminals.rentGate(airline);

                List<AirportFacility> facilities = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service);

                AirportFacility facility = facilities.Find((delegate(AirportFacility f) { return f.TypeLevel == 1; }));

                airport.setAirportFacility(GameObject.GetInstance().HumanAirline, facility,GameObject.GetInstance().GameTime);

                Setup.CreateAirliners();

                Setup.SetupTestGame(opponents);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                PageNavigator.ClearNavigator();

                GameTimer.GetInstance().start();
                PassengerHelpers.CreateDestinationPassengers();

         
                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1001"), string.Format(Translator.GetInstance().GetString("News", "1001", "message"), GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.Name)));
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2403"), Translator.GetInstance().GetString("MessageBox", "2403"), WPFMessageBoxButtons.Ok);

        }
        //sets the airports view
        private void setAirportsView(int year, Country country)
        {
            GameObject.GetInstance().GameTime = new DateTime(year, 1, 1);
                        
            try
            {
                airportsView.Filter = o =>
                {
                    Airport a = o as Airport;
                    return ((Country)new CountryCurrentCountryConverter().Convert(a.Profile.Country)) == country && a.Terminals.getNumberOfGates() > 10 && a.Profile.Period.From<=GameObject.GetInstance().GameTime && a.Profile.Period.To > GameObject.GetInstance().GameTime;
                };
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }

            if (cbAirport.SelectedIndex == -1) cbAirport.SelectedIndex = 0;
        }
      
    }

}
