using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.Helpers;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.General.Countries;
using TheAirline.Views.Airline;
using TheAirline.Views.Game;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageSelectAirports.xaml
    /// </summary>
    public partial class PageSelectAirports : Page, INotifyPropertyChanged
    {
        public StartDataObject StartData{ get; set; }
        public List<Country> AllCountries { get; set; }
        private List<Country> SelectedCountries;

        private int _numberofairports;
        public int NumberOfAirports
        {
            get
            {
                return _numberofairports;
            }
            set
            {
                _numberofairports = value;
                NotifyPropertyChanged("NumberOfAirports");
            }
        }

        public PageSelectAirports(StartDataObject startdata)
        {
            StartData = startdata;
            SelectedCountries = new List<Country>();

            List<Country> countries =
            Airports.GetAllAirports()
                .Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country)
                .Distinct()
                .ToList();

            AllCountries = countries.OrderBy(c=>c.Region.Name).ThenBy(c => c.Name).ToList();

            setNumberOfAirports();

            InitializeComponent();
            
         
        }

        private void lbAirports_Loaded(object sender, RoutedEventArgs e)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(((ListBox)sender).ItemsSource);
            view.GroupDescriptions.Clear();

            var groupDescription = new PropertyGroupDescription("Region.Name");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void cbCountry_Checked(object sender, RoutedEventArgs e)
        {
            Country country = (Country)((CheckBox)sender).Tag;

            SelectedCountries.Add(country);

            setNumberOfAirports();
        }

        private void cbCountry_Unchecked(object sender, RoutedEventArgs e)
        {
            Country country = (Country)((CheckBox)sender).Tag;

            SelectedCountries.Remove(country);

            setNumberOfAirports();
        }
        private void setNumberOfAirports()
        {
            int count = 0;

            if (StartData.MajorAirports)
            {
                int majorAirports = Airports.GetAllAirports(a =>
                                a.Profile.Size == GeneralHelpers.Size.Largest || a.Profile.Size == GeneralHelpers.Size.Large
                                || a.Profile.Size == GeneralHelpers.Size.VeryLarge
                                || a.Profile.Size == GeneralHelpers.Size.Medium).Count;

                foreach (Country country in SelectedCountries)
                {
                    count += Airports.GetAllAirports(a => (new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country) == country && (a.Profile.Size == GeneralHelpers.Size.Small || a.Profile.Size == GeneralHelpers.Size.Smallest
                                || a.Profile.Size == GeneralHelpers.Size.VerySmall)).Count;
                }

                NumberOfAirports = count + majorAirports;
            }
            if (StartData.InternationalAirports)
            {
                int intlAirports = Airports.GetAllAirports(a =>
                               a.Profile.Type == AirportProfile.AirportType.LongHaulInternational 
                               || a.Profile.Type == AirportProfile.AirportType.ShortHaulInternational).Count;

                foreach (Country country in SelectedCountries)
                {
                    count += Airports.GetAllAirports(a => (new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country) == country && (a.Profile.Type == AirportProfile.AirportType.Regional
                        || a.Profile.Type == AirportProfile.AirportType.Domestic)).Count;
                }

                NumberOfAirports = count + intlAirports;
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            foreach (Country country in SelectedCountries)
                StartData.SelectedCountries.Add(country);

            if (!StartData.RandomOpponents)
            {
                PageNavigator.NavigateTo(new PageSelectOpponents(StartData));
            }
            else
            {
                var scCreating = UIHelpers.FindChild<SplashControl>(this, "scCreating");

                if (scCreating != null)
                    scCreating.Visibility = Visibility.Visible;

                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (y, x) => { GameObjectHelpers.CreateGame(StartData); };
                bgWorker.RunWorkerCompleted += (y, x) =>
                {
                    if (scCreating != null)
                        scCreating.Visibility = Visibility.Collapsed;

                    PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                    PageNavigator.ClearNavigator();
                };
                bgWorker.RunWorkerAsync();
            }
        }
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

       

    }
}
