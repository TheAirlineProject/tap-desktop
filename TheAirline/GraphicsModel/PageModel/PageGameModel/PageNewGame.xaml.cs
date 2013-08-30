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
using TheAirline.Model.GeneralModel.HolidaysModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using System.Windows.Controls.Primitives;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using TheAirline.Model.GeneralModel.CountryModel;


namespace TheAirline.GraphicsModel.PageModel.PageGameModel
{
    /// <summary>
    /// Interaction logic for PageStartNewGame.xaml
    /// </summary>
    public partial class PageNewGame : StandardPage
    {
        private TextBox txtName, txtNarrative;
        private TextBlock txtIATA, txtAirlineType;
        private ComboBox cbAirport, cbAirline, cbOpponents, cbStartYear, cbTimeZone, cbDifficulty, cbRegion, cbFocus, cbCountry, cbContinent;
        private ICollectionView airportsView;
        private Rectangle airlineColorRect;
        private Popup popUpSplash;
        private CheckBox cbLocalCurrency, cbDayTurnEnabled, cbSameRegion;
        private ListBox lbContentBasics, lbContentHuman;
        private Button btnCreate, btnBack;
        private enum OpponentSelect { Random, User }
        private OpponentSelect OpponentType;
        public PageNewGame()
        {
            OpponentType = OpponentSelect.Random;

            InitializeComponent();

            popUpSplash = new Popup();

            popUpSplash.Child = UICreator.CreateSplashWindow();
            popUpSplash.Placement = PlacementMode.Center;
            popUpSplash.PlacementTarget = PageNavigator.MainWindow;
            popUpSplash.IsOpen = false;

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

            lbContentBasics = new ListBox();
            lbContentBasics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContentBasics.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbContentBasics.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;

            txtNarrative = new TextBox();
            txtNarrative.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txtNarrative.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            txtNarrative.Background = Brushes.Transparent;
            txtNarrative.TextWrapping = TextWrapping.Wrap;
            txtNarrative.FontStyle = FontStyles.Italic;
            txtNarrative.Width = 500;
            txtNarrative.Height = 100;
            txtNarrative.Uid = "1015";
            txtNarrative.IsReadOnly = true;
            txtNarrative.Text = Translator.GetInstance().GetString("PageNewGame", txtNarrative.Uid);
            txtNarrative.Visibility = System.Windows.Visibility.Collapsed;

            panelContent.Children.Add(txtNarrative);

            panelContent.Children.Add(lbContentBasics);

            lbContentHuman = new ListBox();
            lbContentHuman.Visibility = System.Windows.Visibility.Collapsed;
            lbContentHuman.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbContentHuman.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;
            lbContentHuman.Visibility = System.Windows.Visibility.Collapsed;

            panelContent.Children.Add(lbContentHuman);

            cbContinent = new ComboBox();
            cbContinent.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbContinent.Width = 200;
            cbContinent.DisplayMemberPath = "Name";
            cbContinent.SelectedValuePath = "Name";

            cbContinent.Items.Add(new Continent("100","All continents"));

            foreach (Continent continent in Continents.GetContinents().OrderBy(c => c.Name))
                cbContinent.Items.Add(continent);

            cbContinent.SelectionChanged+=cbContinent_SelectionChanged;
            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame","1022"),cbContinent)); 

            cbRegion = new ComboBox();
            cbRegion.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbRegion.Width = 200;
            cbRegion.DisplayMemberPath = "Name";
            cbRegion.SelectedValuePath = "Name";

            cbRegion.Items.Add(Regions.GetRegion("100"));
            foreach (Region region in Regions.GetRegions().FindAll(r => Airlines.GetAirlines(r).Count > 0).OrderBy(r => r.Name))
                cbRegion.Items.Add(region);

            cbRegion.SelectionChanged += new SelectionChangedEventHandler(cbRegion_SelectionChanged);

            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1012"), cbRegion));
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

            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1002"), panelAirline));

            txtIATA = UICreator.CreateTextBlock("");
            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1003"), txtIATA));

            txtAirlineType = UICreator.CreateTextBlock("");
            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1021"), txtAirlineType));

            StackPanel panelCountry = new StackPanel();

            cbCountry = new ComboBox();
            cbCountry.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCountry.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbCountry.Width = 150;
            cbCountry.SelectionChanged += cbCountry_SelectionChanged;

            panelCountry.Children.Add(cbCountry);

            cbLocalCurrency = new CheckBox();
            cbLocalCurrency.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            cbLocalCurrency.Content = Translator.GetInstance().GetString("PageNewGame", "1014");
            cbLocalCurrency.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            cbLocalCurrency.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            panelCountry.Children.Add(cbLocalCurrency);

            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1004"), panelCountry));


            txtName = new TextBox();
            txtName.Background = Brushes.Transparent;
            txtName.BorderBrush = Brushes.Black;
            txtName.Width = 200;


            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1005"), txtName));

            // chs, 2011-19-10 added to show the airline color
            airlineColorRect = new Rectangle();
            airlineColorRect.Width = 40;
            airlineColorRect.Height = 20;
            airlineColorRect.StrokeThickness = 1;
            airlineColorRect.Stroke = Brushes.Black;
            airlineColorRect.Fill = new AirlineBrushConverter().Convert(Airlines.GetAirline("ZA")) as Brush;
            airlineColorRect.Margin = new Thickness(0, 2, 0, 2);

            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1006"), airlineColorRect));

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

            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1007"), cbAirport));

            cbStartYear = new ComboBox();
            cbStartYear.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbStartYear.Width = 60;
            for (int i = GameObject.StartYear; i < DateTime.Now.Year + 2; i++)
                cbStartYear.Items.Add(i);

            cbStartYear.SelectionChanged += new SelectionChangedEventHandler(cbStartYear_SelectionChanged);

            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1008"), cbStartYear));

            cbTimeZone = new ComboBox();
            cbTimeZone.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbTimeZone.Width = 300;
            cbTimeZone.DisplayMemberPath = "DisplayName";
            cbTimeZone.SelectedValuePath = "DisplayName";


            foreach (GameTimeZone gtz in TimeZones.GetTimeZones())
                cbTimeZone.Items.Add(gtz);

            cbTimeZone.SelectedItem = TimeZones.GetTimeZones().Find(delegate(GameTimeZone gtz) { return gtz.UTCOffset == new TimeSpan(0, 0, 0); });

            lbContentHuman.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1009"), cbTimeZone));

            cbFocus = new ComboBox();
            cbFocus.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbFocus.Width = 100;

            foreach (Airline.AirlineFocus focus in Enum.GetValues(typeof(Airline.AirlineFocus)))
                cbFocus.Items.Add(focus);

            cbFocus.SelectedIndex = 0;

            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1013"), cbFocus));

            WrapPanel panelDifficulty = new WrapPanel();

            cbDifficulty = new ComboBox();
            cbDifficulty.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDifficulty.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbDifficulty.Width = 100;
            cbDifficulty.DisplayMemberPath = "Name";
            cbDifficulty.SelectedValuePath = "Name";

            foreach (DifficultyLevel difficulty in DifficultyLevels.GetDifficultyLevels())
                cbDifficulty.Items.Add(difficulty);

            cbDifficulty.SelectedIndex = 0;

            panelDifficulty.Children.Add(cbDifficulty);

            Button btnAddDifficulty = new Button();
            btnAddDifficulty.Margin = new Thickness(5, 0, 0, 0);
            btnAddDifficulty.Background = Brushes.Transparent;
            btnAddDifficulty.Click += new RoutedEventHandler(btnAddDifficulty_Click);

            Image imgAddDifficulty = new Image();
            imgAddDifficulty.Source = new BitmapImage(new Uri(@"/Data/images/add.png", UriKind.RelativeOrAbsolute));
            imgAddDifficulty.Height = 16;
            RenderOptions.SetBitmapScalingMode(imgAddDifficulty, BitmapScalingMode.HighQuality);

            btnAddDifficulty.Content = imgAddDifficulty;

            panelDifficulty.Children.Add(btnAddDifficulty);

            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1011"), panelDifficulty));

            WrapPanel panelOpponents = new WrapPanel();

            cbOpponents = new ComboBox();
            cbOpponents.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbOpponents.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbOpponents.Width = 50;
            cbOpponents.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            for (int i = 0; i < Airlines.GetAllAirlines().Count; i++)
                cbOpponents.Items.Add(i);

            cbOpponents.SelectedIndex = 3;

            panelOpponents.Children.Add(cbOpponents);

            cbSameRegion = new CheckBox();
            cbSameRegion.FlowDirection = System.Windows.FlowDirection.RightToLeft;
            cbSameRegion.Content = Translator.GetInstance().GetString("PageNewGame", "1017");
            cbSameRegion.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            cbSameRegion.Margin = new Thickness(5, 0, 0, 0);

            panelOpponents.Children.Add(cbSameRegion);

            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1010"), panelOpponents));

            WrapPanel panelOpponentSelect = new WrapPanel();

            RadioButton rbRandomOpponents = new RadioButton();
            rbRandomOpponents.IsChecked = true;
            rbRandomOpponents.GroupName = "Opponent";
            rbRandomOpponents.Content = Translator.GetInstance().GetString("PageNewGame", "1018");
            rbRandomOpponents.Checked+=rbRandomOpponents_Checked;

            panelOpponentSelect.Children.Add(rbRandomOpponents);

            RadioButton rbSelectOpponents = new RadioButton();
            rbSelectOpponents.GroupName = "Opponent";
            rbSelectOpponents.Content = Translator.GetInstance().GetString("PageNewGame", "1019");
            rbSelectOpponents.Checked +=rbSelectOpponents_Checked;
            rbSelectOpponents.Margin = new Thickness(5, 0, 0, 0);
            
            panelOpponentSelect.Children.Add(rbSelectOpponents);

            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame","1020"),panelOpponentSelect));

            cbDayTurnEnabled = new CheckBox();
            cbDayTurnEnabled.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            cbDayTurnEnabled.IsChecked = true;

            lbContentBasics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageNewGame", "1016"), cbDayTurnEnabled));

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelContent.Children.Add(panelButtons);

            btnCreate = new Button();
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Uid = "203";
            btnCreate.Content = Translator.GetInstance().GetString("PageNewGame", btnCreate.Uid);
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnCreate);

            btnBack = new Button();
            btnBack.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnBack.Height = Double.NaN;
            btnBack.Width = Double.NaN;
            btnBack.Uid = "119";
            btnBack.Content = Translator.GetInstance().GetString("General",btnBack.Uid);
            btnBack.SetResourceReference(Button.BackgroundProperty,"ButtonBrush");
            btnBack.Margin = new Thickness(5, 0, 0, 0);
            btnBack.Click += btnBack_Click;
            btnBack.Visibility = System.Windows.Visibility.Collapsed;
            panelButtons.Children.Add(btnBack);

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

            cbStartYear.SelectedItem = DateTime.Now.Year;



            showPage(this);



        }

        
      
        private void rbSelectOpponents_Checked(object sender, RoutedEventArgs e)
        {
            this.OpponentType = OpponentSelect.User;
        }

        void rbRandomOpponents_Checked(object sender, RoutedEventArgs e)
        {
            this.OpponentType = OpponentSelect.Random;
        }

        private void cbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Airline airline = (Airline)cbAirline.SelectedItem;
            Country country = (Country)cbCountry.SelectedItem;
            int year = (int)cbStartYear.SelectedItem;

            if (country != null)
            {
                setAirportsView(year, country);

                if (airline.Profile.PreferedAirport != null && cbAirport.Items.Contains(airline.Profile.PreferedAirport))
                    cbAirport.SelectedItem = airline.Profile.PreferedAirport;
                else
                {
                    var aa = cbAirport.Items.Cast<Airport>().ToList();
                    Airport homeAirport = aa.Find(a => a.Profile.Country == country);

                    cbAirport.SelectedItem = homeAirport == null ? cbAirport.Items[0] : homeAirport;
                }
            }

        }
        private void cbContinent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Continent continent = (Continent)cbContinent.SelectedItem;

            if (continent.Regions.Count == 0)
            {
                cbRegion.Items.Clear();

                cbRegion.Items.Add(Regions.GetRegion("100"));
                foreach (Region region in Regions.GetRegions().FindAll(r => Airlines.GetAirlines(r).Count > 0).OrderBy(r => r.Name)) 
                    cbRegion.Items.Add(region);

            }
            else
            {
                cbRegion.Items.Clear();

                if (continent.Regions.Count > 1)
                    cbRegion.Items.Add(Regions.GetRegion("100"));
                
                foreach (Region region in continent.Regions.FindAll(r => Airlines.GetAirlines(r).Count > 0).OrderBy(r => r.Name))
                    cbRegion.Items.Add(region);

            }
        }

        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Region region = (Region)cbRegion.SelectedItem;
            int year = (int)cbStartYear.SelectedItem;
            
            Continent continent = (Continent)cbContinent.SelectedItem;

            if (continent == null)
            {
                cbContinent.SelectedIndex = 0;
                continent = (Continent)cbContinent.SelectedItem;
            }

            if (region == null)
            {
                cbRegion.SelectedIndex = 0;
                region = (Region)cbRegion.SelectedItem;
            }

            if (region != null)
            {
                var source = cbAirline.Items as ICollectionView;
                source.Filter = delegate(object item)
                {
                    var airline = item as Airline;
                    return (airline.Profile.Country.Region == region || (region.Uid == "100" && continent.Uid == "100") || (region.Uid == "100" && continent.hasRegion(airline.Profile.Country.Region))) && airline.Profile.Founded <= year && airline.Profile.Folded > year;

                };
                source.Refresh();

                cbAirline.SelectedIndex = 0;


                cbOpponents.Items.Clear();

                for (int i = 0; i < cbAirline.Items.Count; i++)
                    cbOpponents.Items.Add(i);

                cbOpponents.SelectedIndex = Math.Min(cbOpponents.Items.Count - 1, 3);
            }
        }


        private void cbStartYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Continent continent = (Continent)cbContinent.SelectedItem;

            if (continent == null)
            {
                cbContinent.SelectedIndex = 0;
                continent = (Continent)cbContinent.SelectedItem;
            }

            Region region = (Region)cbRegion.SelectedItem;
            if (region == null)
            {
                cbRegion.SelectedIndex = 0;
                region = (Region)cbRegion.SelectedItem;
            }

            int year = (int)cbStartYear.SelectedItem;
            
            var source = cbAirline.Items as ICollectionView;
            source.Filter = delegate(object item)
            {
                var airline = item as Airline;
                return (airline.Profile.Country.Region == region || (region.Uid == "100" && continent.Uid == "100") || (region.Uid == "100" && continent.hasRegion(airline.Profile.Country.Region))) && airline.Profile.Founded <= year && airline.Profile.Folded > year;

            };

            source.Refresh();

            cbAirline.SelectedIndex = 0;

            setAirportsView(year, ((Airline)cbAirline.SelectedItem).Profile.Country);

            cbOpponents.Items.Clear();

            for (int i = 0; i < cbAirline.Items.Count; i++)
                cbOpponents.Items.Add(i);

            cbOpponents.SelectedIndex = Math.Min(cbOpponents.Items.Count - 1, 3);

        }

        private void cbDifficulty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Airline airline = (Airline)cbDifficulty.SelectedItem;
            int year = (int)cbStartYear.SelectedItem;

            setAirportsView(year, airline.Profile.Country);
        }



        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            Airline airline = (Airline)cbAirline.SelectedItem;

            if (airline != null)
            {
                int year = (int)cbStartYear.SelectedItem;


                airlineColorRect.Fill = new AirlineBrushConverter().Convert(airline) as Brush;
                txtName.Text = airline.Profile.CEO;
                txtIATA.Text = airline.Profile.IATACode;
                txtAirlineType.Text = airline.AirlineRouteFocus.ToString();

                cbCountry.Items.Clear();

                foreach (Country country in airline.Profile.Countries)
                    cbCountry.Items.Add(country);

                cbCountry.SelectedIndex = 0;

                //cntCountry.Content = airline.Profile.Country;
                cbLocalCurrency.Visibility = airline.Profile.Country.Currencies.Count > 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                cbLocalCurrency.IsChecked = airline.Profile.Country.Currencies.Count > 0;

                txtNarrative.Text = airline.Profile.Narrative;
            }

        }
        private void btnAddAirline_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewAirline());

        }
        private void btnAddDifficulty_Click(object sender, RoutedEventArgs e)
        {
            
            object o = PopUpDifficulty.ShowPopUp((DifficultyLevel)cbDifficulty.SelectedItem);

            if (o != null && o is DifficultyLevel)
            {
                DifficultyLevel level = (DifficultyLevel)o;

                if (DifficultyLevels.GetDifficultyLevel("Custom") != null)
                {
                    DifficultyLevel customLevel = DifficultyLevels.GetDifficultyLevel("Custom");

                    DifficultyLevels.RemoveDifficultyLevel(customLevel);
                    cbDifficulty.Items.Remove(customLevel);
                }

                DifficultyLevels.AddDifficultyLevel(level);

                cbDifficulty.Items.Add(level);
                cbDifficulty.SelectedItem = level;
            }
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
        public void DoEvents()
        {
            DispatcherFrame f = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            (SendOrPostCallback)delegate(object arg)
            {
                DispatcherFrame fr = arg as DispatcherFrame;
                fr.Continue = false;
            }, f);
            Dispatcher.PushFrame(f);
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            lbContentBasics.Visibility = System.Windows.Visibility.Visible;
            lbContentHuman.Visibility = System.Windows.Visibility.Collapsed;

            btnBack.Visibility = System.Windows.Visibility.Collapsed;
            txtNarrative.Visibility = System.Windows.Visibility.Collapsed;

            btnCreate.Content = Translator.GetInstance().GetString("PageNewGame", "203");
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (lbContentBasics.Visibility == System.Windows.Visibility.Collapsed)
            {
                createNewGame();
              
            }
            if (lbContentBasics.Visibility == System.Windows.Visibility.Visible)
            {
                btnCreate.Content = Translator.GetInstance().GetString("PageNewGame", "201");
        
                lbContentBasics.Visibility = System.Windows.Visibility.Collapsed;
                lbContentHuman.Visibility = System.Windows.Visibility.Visible;

                txtNarrative.Visibility = System.Windows.Visibility.Visible;
                btnBack.Visibility = System.Windows.Visibility.Visible;
            }
            


  
        }
        //creates a new game
        private void createNewGame()
        {
            if (txtName.Text.Trim().Length > 2)
            {

                object o = null;
                int startYear = (int)cbStartYear.SelectedItem;
                int opponents = (int)cbOpponents.SelectedItem;
                Airline airline = (Airline)cbAirline.SelectedItem;
                Continent continent = (Continent)cbContinent.SelectedItem;
                Region region = (Region)cbRegion.SelectedItem;


                if (this.OpponentType == OpponentSelect.User)
                {
                    if (cbSameRegion.IsChecked.Value)
                        o = PopUpSelectOpponents.ShowPopUp(airline, opponents, startYear, airline.Profile.Country.Region);
                    else
                        o = PopUpSelectOpponents.ShowPopUp(airline, opponents, startYear, region,continent);
                }



                // popUpSplash.IsOpen = true;

                DoEvents();

                GameTimeZone gtz = (GameTimeZone)cbTimeZone.SelectedItem;
                GameObject.GetInstance().DayRoundEnabled = cbDayTurnEnabled.IsChecked.Value;
                GameObject.GetInstance().TimeZone = gtz;
                GameObject.GetInstance().Difficulty = (DifficultyLevel)cbDifficulty.SelectedItem;
                GameObject.GetInstance().GameTime = new DateTime(startYear, 1, 1);
                GameObject.GetInstance().StartDate = GameObject.GetInstance().GameTime;
                //sets the fuel price
                GameObject.GetInstance().FuelPrice = Inflations.GetInflation(GameObject.GetInstance().GameTime.Year).FuelPrice;

                airline.Profile.Country = (Country)cbCountry.SelectedItem;
                airline.Profile.CEO = txtName.Text.Trim();

                GameObject.GetInstance().HumanAirline = airline;
                GameObject.GetInstance().MainAirline = GameObject.GetInstance().HumanAirline;

                if (cbLocalCurrency.IsChecked.Value)
                    GameObject.GetInstance().CurrencyCountry = airline.Profile.Country;
                // AppSettings.GetInstance().resetCurrencyFormat();

                Airport airport = (Airport)cbAirport.SelectedItem;

                AirportHelpers.RentGates(airport, airline, 2);

                AirportFacility checkinFacility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.CheckIn).Find(f => f.TypeLevel == 1);
                AirportFacility facility = AirportFacilities.GetFacilities(AirportFacility.FacilityType.Service).Find((delegate(AirportFacility f) { return f.TypeLevel == 1; }));

                airport.addAirportFacility(GameObject.GetInstance().HumanAirline, facility, GameObject.GetInstance().GameTime);
                airport.addAirportFacility(GameObject.GetInstance().HumanAirline, checkinFacility, GameObject.GetInstance().GameTime);

                if (continent.Uid != "100" || region.Uid != "100")
                {
                    var airlines =  Airlines.GetAirlines(a=>a.Profile.Country.Region == region ||  (region.Uid == "100" && continent.hasRegion(a.Profile.Country.Region)) && a.Profile.Founded <= startYear && a.Profile.Folded > startYear);
                    var airports = Airports.GetAirports(a => a.Profile.Country.Region == region || (region.Uid == "100" && continent.hasRegion(a.Profile.Country.Region)) && a.Profile.Period.From.Year <= startYear && a.Profile.Period.To.Year > startYear);
                    
                    //Airports.RemoveAirports(a => (a.Profile.Country.Region != region && !continent.hasRegion(a.Profile.Country.Region)) || (a.Profile.Town.State != null && a.Profile.Town.State.IsOverseas));
                    Airports.Clear();
                    foreach (Airport a in airports)
                        Airports.AddAirport(a);

                    Airlines.Clear();
                    foreach (Airline a in airlines)
                        Airlines.AddAirline(a);
                }

                PassengerHelpers.CreateAirlineDestinationDemand();

                AirlinerHelpers.CreateStartUpAirliners();

                if (this.OpponentType == OpponentSelect.Random || o == null)
                    Setup.SetupMainGame(opponents, cbSameRegion.IsChecked.Value);
                else
                    Setup.SetupMainGame((List<Airline>)o);


                airline.MarketFocus = (Airline.AirlineFocus)cbFocus.SelectedItem;

                GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

                //PassengerHelpers.CreateDestinationPassengers();

                GameTimer.GetInstance().start();
                GameObjectWorker.GetInstance().start();
                // AIWorker.GetInstance().start();

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                PageNavigator.ClearNavigator();

                //GameObject.GetInstance().HumanAirline.Money = 10000000000000;

                GameObject.GetInstance().NewsBox.addNews(new News(News.NewsType.Standard_News, GameObject.GetInstance().GameTime, Translator.GetInstance().GetString("News", "1001"), string.Format(Translator.GetInstance().GetString("News", "1001", "message"), GameObject.GetInstance().HumanAirline.Profile.CEO, GameObject.GetInstance().HumanAirline.Profile.IATACode)));

                popUpSplash.IsOpen = false;



                Action action = () =>
                {
                    Stopwatch swPax = new Stopwatch();
                    swPax.Start();

                    PassengerHelpers.CreateDestinationDemand();

                    Console.WriteLine("Demand have been created in {0} ms.", swPax.ElapsedMilliseconds);
                    swPax.Stop();
                };

                Task.Factory.StartNew(action);
                //Task.Run(action);
                //Task t2 = Task.Factory.StartNew(action, "passengers");


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
                    return ((Country)new CountryCurrentCountryConverter().Convert(a.Profile.Country)) == (Country)new CountryCurrentCountryConverter().Convert(country) && (a.Profile.Town.State == null || !a.Profile.Town.State.IsOverseas) && GeneralHelpers.IsAirportActive(a);// && a.Terminals.getNumberOfGates() > 10 //a.Profile.Period.From<=GameObject.GetInstance().GameTime && a.Profile.Period.To > GameObject.GetInstance().GameTime;
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
