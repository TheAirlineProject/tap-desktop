using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using Microsoft.Win32;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.CustomControlsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.ObjectsModel;
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageAirlineData.xaml
    /// </summary>
    public partial class PageAirlineData : Page
    {
        #region Fields

        private readonly StartDataObject StartData;

        #endregion

        #region Constructors and Destructors

        public PageAirlineData(StartDataObject startData)
        {
            this.AllTimeZones = TimeZones.GetTimeZones();
            this.AllAirports = new ObservableCollection<Airport>();
            this.StartData = startData;

            GameObject.GetInstance().GameTime = new DateTime(this.StartData.Year, 1, 1);

            this.InitializeComponent();

            List<Airline> airlines =
                Airlines.GetAirlines(
                    airline =>
                        (airline.Profile.Country.Region == this.StartData.Region
                         || (this.StartData.Region.Uid == "100" && this.StartData.Continent.Uid == "100")
                         || (this.StartData.Region.Uid == "100"
                             && this.StartData.Continent.HasRegion(airline.Profile.Country.Region)))
                        && airline.Profile.Founded <= this.StartData.Year
                        && airline.Profile.Folded > this.StartData.Year).OrderBy(a => a.Profile.Name).ToList();

            this.cbAirline.ItemsSource = airlines;
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Airport> AllAirports { get; set; }

        public List<GameTimeZone> AllTimeZones { get; set; }

        #endregion

        #region Methods

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var frmContent = UIHelpers.FindChild<Frame>((Page)this.Tag, "frmContent");

            frmContent.Navigate(new PageStartData { Tag = this.Tag });
        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            this.StartData.Airline = (Airline)this.cbAirline.SelectedItem;
            this.StartData.Airport = (Airport)this.cbAirport.SelectedItem;
            this.StartData.CEO = this.txtCEO.Text;
            this.StartData.HomeCountry = (Country)this.cbCountry.SelectedItem;
            this.StartData.TimeZone = (GameTimeZone)this.cbTimeZone.SelectedItem;
            this.StartData.LocalCurrency = this.cbLocalCurrency.IsChecked.Value
                                           && this.StartData.HomeCountry.HasLocalCurrency;

            if (this.StartData.SelectedCountries != null)
            {
                PageNavigator.NavigateTo(new PageSelectAirports(this.StartData));
            }
            else if (!this.StartData.RandomOpponents)
            {
                PageNavigator.NavigateTo(new PageSelectOpponents(this.StartData));
            }
            else
            {
                var scCreating = UIHelpers.FindChild<SplashControl>(this, "scCreating");

                scCreating.Visibility = Visibility.Visible;

                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (y, x) => { GameObjectHelpers.CreateGame(this.StartData); };
                bgWorker.RunWorkerCompleted += (y, x) =>
                {
                    scCreating.Visibility = Visibility.Collapsed;

                    PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                    PageNavigator.ClearNavigator();
                };
                bgWorker.RunWorkerAsync();
            }
        }

        private void btnLoadAirline_Click(object sender, RoutedEventArgs e)
        {
            string directory = AppSettings.GetCommonApplicationDataPath() + "\\custom airlines";

            var dlg = new OpenFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "Airline XMLs (.xml)|*.xml";
            dlg.InitialDirectory = Path.GetFullPath(directory);
            dlg.Multiselect = false;

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string path = dlg.FileName;

                Airline airline = Setup.LoadAirline(path);

                string imagePath = string.Format("{0}\\{1}.png", directory, airline.Profile.IATACode);

                if (File.Exists(imagePath))
                {
                    airline.Profile.AddLogo(new AirlineLogo(imagePath));
                }
                else
                {
                    airline.Profile.AddLogo(
                        new AirlineLogo(AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\default.png"));
                }

                if (Airlines.GetAirline(airline.Profile.IATACode) != null)
                {
                    Airlines.RemoveAirlines(a => a.Profile.IATACode == airline.Profile.IATACode);
                }

                Airlines.AddAirline(airline);

                List<Airline> airlines =
                    Airlines.GetAirlines(
                        a =>
                            (a.Profile.Country.Region == this.StartData.Region
                             || (this.StartData.Region.Uid == "100" && this.StartData.Continent.Uid == "100")
                             || (this.StartData.Region.Uid == "100"
                                 && this.StartData.Continent.HasRegion(a.Profile.Country.Region)))
                            && a.Profile.Founded <= this.StartData.Year && a.Profile.Folded > this.StartData.Year)
                        .OrderBy(a => a.Profile.Name)
                        .ToList();

                this.cbAirline.ItemsSource = airlines;

                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2409"),
                    Translator.GetInstance().GetString("MessageBox", "2409", "message"),
                    WPFMessageBoxButtons.Ok);

                if (this.cbAirline.Items.Contains(airline))
                {
                    this.cbAirline.SelectedItem = airline;
                }
            }
        }

        private void btnStartMenu_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var airline = (Airline)this.cbAirline.SelectedItem;

            this.AllAirports.Clear();

            foreach (
                Airport airport in
                    Airports.GetAllActiveAirports()
                        .Where(a => airline.Profile.Countries.Contains(a.Profile.Country))
                        .OrderBy(a => a.Profile.Name))
            {
                this.AllAirports.Add(airport);
            }

            if (this.AllAirports.Contains(airline.Profile.PreferedAirport))
            {
                this.cbAirport.SelectedItem = airline.Profile.PreferedAirport;
            }
            else
            {
                this.cbAirport.SelectedIndex = 0;
            }
        }

        #endregion
    }
}