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
            txtHeader.Text = "Airline Profile";
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

            List<Airline> airlines = Airlines.GetAirlines();
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

            lbContent.Items.Add(new QuickInfoValue("Select Airline", panelAirline));

            txtIATA = UICreator.CreateTextBlock("");
            lbContent.Items.Add(new QuickInfoValue("IATA Code", txtIATA));

            cntCountry = new ContentControl();
            cntCountry.SetResourceReference(ContentControl.ContentTemplateProperty, "CountryFlagLongItem");
    
            lbContent.Items.Add(new QuickInfoValue("Home country", cntCountry));


            txtName = new TextBox();
            txtName.Background = Brushes.Transparent;
            txtName.BorderBrush = Brushes.Black;
            txtName.Width = 200;


            lbContent.Items.Add(new QuickInfoValue("Name of CEO", txtName));

            // chs, 2011-19-10 added to show the airline color
            airlineColorRect = new Rectangle();
            airlineColorRect.Width = 40;
            airlineColorRect.Height = 20;
            airlineColorRect.StrokeThickness = 1;
            airlineColorRect.Stroke = Brushes.Black;
            airlineColorRect.Fill = new AirlineBrushConverter().Convert(Airlines.GetAirline("ZA")) as Brush;
            airlineColorRect.Margin = new Thickness(0, 2, 0, 2);

            lbContent.Items.Add(new QuickInfoValue("Airline Color", airlineColorRect));

            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirport.IsSynchronizedWithCurrentItem = true;
            cbAirport.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirport.SelectionChanged += new SelectionChangedEventHandler(cbAirports_SelectionChanged);
    
            List<Airport> airportsList = Airports.GetAirports();

            airportsView = CollectionViewSource.GetDefaultView(airportsList);
            airportsView.SortDescriptions.Add(new SortDescription("Profile.Name", ListSortDirection.Ascending));

            cbAirport.ItemsSource = airportsView;

     
            lbContent.Items.Add(new QuickInfoValue("Select homebase", cbAirport));

   
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
            panelContent.Children.Add(panelButtons);


            Button btnCreate = new Button();
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            //btnCreate.Padding = new Thickness(2, 2,2,2);
            btnCreate.Content = "Create Game";
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnCreate);

            Button btnExit = new Button();
            btnExit.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnExit.Height = double.NaN;
            btnExit.Width = double.NaN;
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

                int opponents = (int)cbOpponents.SelectedItem;
                Airline airline = (Airline)cbAirline.SelectedItem;

                airline.Profile.CEO = txtName.Text.Trim();

                GameObject.GetInstance().HumanAirline = airline;

                Airport airport = (Airport)cbAirport.SelectedItem;

                airport.Terminals.rentGate(airline);
                airport.Terminals.rentGate(airline);
                                
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

            try
            {
                airportsView.Filter = o =>
                {
                    Airport a = o as Airport;
                    return a.Profile.Country.Region == airline.Profile.Country.Region && (a.Profile.Size == AirportProfile.AirportSize.Smallest || a.Profile.Size == AirportProfile.AirportSize.Very_small);
                };
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
            airlineColorRect.Fill = new AirlineBrushConverter().Convert(airline) as Brush;
            txtName.Text = airline.Profile.CEO;
            txtIATA.Text = airline.Profile.IATACode;
            cntCountry.Content = airline.Profile.Country;

        }

    }

}
