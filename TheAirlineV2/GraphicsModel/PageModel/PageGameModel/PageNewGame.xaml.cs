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
using TheAirlineV2.GraphicsModel.PageModel.GeneralModel;
using TheAirlineV2.Model.AirlineModel;
using TheAirlineV2.Model.AirportModel;
using TheAirlineV2.Model.GeneralModel;
using TheAirlineV2.GraphicsModel.PageModel.PageAirlineModel;
using TheAirlineV2.GraphicsModel.UserControlModel.MessageBoxModel;
using System.ComponentModel;
using TheAirlineV2.GraphicsModel.UserControlModel;
using TheAirlineV2.Model.AirlinerModel;

namespace TheAirlineV2.GraphicsModel.PageModel.PageGameModel
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
            txtHeader.Text = "Airline Profile";
            panelContent.Children.Add(txtHeader);


            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            // lbAirlines.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;

            panelContent.Children.Add(lbContent);

            cbAirline = new ComboBox();
            //cbAirlines.Background = Brushes.Yellow;
            cbAirline.SetResourceReference(ComboBox.ItemTemplateProperty, "AirlineLogoItem");
            cbAirline.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirline.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirline.SelectionChanged += new SelectionChangedEventHandler(cbAirline_SelectionChanged);
            //cbAirline.DisplayMemberPath = "Profile.Name";
            //cbAirline.SelectedValuePath = "Profile.Name";
            cbAirline.Width = 200;

            List<Airline> airlines = Airlines.GetAirlines();
            airlines.Sort((delegate(Airline a1, Airline a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); }));

            cbAirline.ItemsSource = airlines;


            lbContent.Items.Add(new QuickInfoValue("Select Airline", cbAirline));

            txtIATA = UICreator.CreateTextBlock("");
            lbContent.Items.Add(new QuickInfoValue("IATA Code", txtIATA));

            cntCountry = new ContentControl();
            cntCountry.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
            //lblFlag.Content = this.Airline.Profile.Country;

            lbContent.Items.Add(new QuickInfoValue("Home country", cntCountry));


            txtName = new TextBox();
            txtName.Background = Brushes.Transparent;
            txtName.BorderBrush = Brushes.Black;
            txtName.Width = 200;


            lbContent.Items.Add(new QuickInfoValue("Name of CEO", txtName));


            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            //cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.IsSynchronizedWithCurrentItem = true;
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirport.SelectionChanged += new SelectionChangedEventHandler(cbAirports_SelectionChanged);
            //cbAirports.IsEditable = false;
            //bAirports.IsReadOnly = true;

            List<Airport> airportsList = Airports.GetAirports();

            airportsView = CollectionViewSource.GetDefaultView(airportsList);
            airportsView.SortDescriptions.Add(new SortDescription("Profile.Name", ListSortDirection.Ascending));

            //cbAirports.ItemsSource = getAirportsList();// airportsView;
            cbAirport.ItemsSource = airportsView;

     
            lbContent.Items.Add(new QuickInfoValue("Select homebase", cbAirport));

            //List<AirlinerType> types = AirlinerTypes.GetTypes();
            //types.Sort(delegate(AirlinerType a1, AirlinerType a2) { return a1.Produced.From.CompareTo(a2.Produced.From); });

            cbStartYear = new ComboBox();
            cbStartYear.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbStartYear.Width = 60;
            for (int i = 1960; i < DateTime.Now.Year+2; i++)
                cbStartYear.Items.Add(i);

            cbStartYear.SelectedItem = DateTime.Now.Year;

            lbContent.Items.Add(new QuickInfoValue("Select start year", cbStartYear));

            cbTimeZone = new ComboBox();
            cbTimeZone.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbTimeZone.Width = 300;
            cbTimeZone.DisplayMemberPath = "DisplayName";
            cbTimeZone.SelectedValuePath = "DisplayName";
            

            foreach (GameTimeZone gtz in TimeZones.GetTimeZones())
                cbTimeZone.Items.Add(gtz);

            cbTimeZone.SelectedItem = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) {return gtz.UTCOffset == new TimeSpan(0,0,0);});

            lbContent.Items.Add(new QuickInfoValue("Select time zone", cbTimeZone));



            cbOpponents = new ComboBox();
            cbOpponents.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbOpponents.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbOpponents.Width = 50;
            cbOpponents.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            for (int i = 2; i<Math.Min(Airlines.GetAirlines().Count,10); i++)
                cbOpponents.Items.Add(i);

            cbOpponents.SelectedIndex = cbOpponents.Items.Count - 1;

            lbContent.Items.Add(new QuickInfoValue("Select # of opponents", cbOpponents));

          
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            //panelButtons.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            panelContent.Children.Add(panelButtons);


            Button btnCreate = new Button();
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);
            btnCreate.Height = 20;
            btnCreate.Width = 80;
            btnCreate.Content = "Create Game";
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            //btnPrevious.Click += new RoutedEventHandler(btnPrevious_Click);
            panelButtons.Children.Add(btnCreate);

            Button btnExit = new Button();
            btnExit.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnExit.Height = 20;
            btnExit.Width = 80;
            btnExit.Content = "Start Menu";
            btnExit.Margin = new Thickness(5, 0, 0, 0);
            btnExit.Click += new RoutedEventHandler(btnCancel_Click);
            btnExit.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnExit);

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent("Create New Game");

            cbAirline.SelectedIndex = 0;


            showPage(this);
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
           // WPFMessageBoxResult result = WPFMessageBox.Show("Exit game", "Are you sure you want to exit the game?", WPFMessageBoxButtons.YesNo);

            //if (result == WPFMessageBoxResult.Yes)
                //PageNavigator.MainWindow.Close();
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

                int opponents = (int)cbOpponents.SelectedItem;
                Airline airline = (Airline)cbAirline.SelectedItem;

                airline.Profile.CEO = txtName.Text.Trim();

                GameObject.GetInstance().HumanAirline = airline;

                Airport airport = (Airport)cbAirport.SelectedItem;

                airport.Gates.rentGate(airline);
                airport.Gates.rentGate(airline);
                                
                List<AirportFacility> facilities = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service);
                
                AirportFacility facility =  facilities.Find((delegate(AirportFacility f) { return f.TypeLevel==1; }));

                airport.setAirportFacility(GameObject.GetInstance().HumanAirline, facility);


                Setup.CreateAirliners();

                Setup.SetupTestGame(opponents);

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                PageNavigator.ClearNavigator();

                GameTimer.GetInstance().start();

                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, "Welcome to the game", string.Format("Dear {0},\n\nWelcome as the CEO of {1}. I hope you will enjoy the game",GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.Name)));
            }
            else
                WPFMessageBox.Show("Name of CEO", "The name of the CEO must be longer than 2 characters", WPFMessageBoxButtons.Ok);

        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            Airline airline = (Airline)cbAirline.SelectedItem;


            airportsView.Filter = o =>
            {
                Airport a = o as Airport;
                return a.Profile.Country.Region == airline.Profile.Country.Region && (a.Profile.Size == AirportProfile.AirportSize.Smallest || a.Profile.Size == AirportProfile.AirportSize.Very_small);
            };

            //cbAirports.Items.Clear();
            //Airports.GetAirports(airline.Profile.Country.Region);
            // foreach (Airport airport in Airports.GetAirports(airline.Profile.Country.Region))
            //   cbAirports.Items.Add(airport);



            txtName.Text = airline.Profile.CEO;
            //lblFlag.Content = this.Airline.Profile.Country;
            txtIATA.Text = airline.Profile.IATACode;
            cntCountry.Content = airline.Profile.Country;

        }

    }

}
