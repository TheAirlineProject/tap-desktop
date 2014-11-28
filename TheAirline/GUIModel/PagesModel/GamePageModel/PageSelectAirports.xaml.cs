using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageSelectAirports.xaml
    /// </summary>
    public partial class PageSelectAirports : Page, INotifyPropertyChanged
    {
        public StartDataObject StartData{ get; set; }
        public ObservableCollection<Country> AllCountries { get; set; }
        private ObservableCollection<Country> SelectedCountries;

        private int _numberofairports;
        public int NumberOfAirports
        {
            get
            {
                return this._numberofairports;
            }
            set
            {
                this._numberofairports = value;
                this.NotifyPropertyChanged("NumberOfAirports");
            }
        }

        public PageSelectAirports(StartDataObject startdata)
        {
            this.StartData = startdata;
            this.SelectedCountries = new ObservableCollection<Country>();

            List<Country> countries =
            Airports.GetAllAirports()
                .Select(a => new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country)
                .Distinct()
                .ToList();

            this.AllCountries = new ObservableCollection<Country>(countries.OrderBy(c=>c.Region.Name).ThenBy(c => c.Name));

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

            this.SelectedCountries.Add(country);

            setNumberOfAirports();
        }

        private void cbCountry_Unchecked(object sender, RoutedEventArgs e)
        {
            Country country = (Country)((CheckBox)sender).Tag;

            this.SelectedCountries.Remove(country);

            setNumberOfAirports();
        }
        private void setNumberOfAirports()
        {
            int count = 0;

            if (this.StartData.MajorAirports)
            {
                int majorAirports = Airports.GetAllAirports(a =>
                                a.Profile.Size == GeneralHelpers.Size.Largest || a.Profile.Size == GeneralHelpers.Size.Large
                                || a.Profile.Size == GeneralHelpers.Size.Very_large
                                || a.Profile.Size == GeneralHelpers.Size.Medium).Count;

                foreach (Country country in this.SelectedCountries)
                {
                    count += Airports.GetAllAirports(a => (new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country) == country && (a.Profile.Size == GeneralHelpers.Size.Small || a.Profile.Size == GeneralHelpers.Size.Smallest
                                || a.Profile.Size == GeneralHelpers.Size.Very_small)).Count;
                }

                this.NumberOfAirports = count + majorAirports;
            }
            if (this.StartData.InternationalAirports)
            {
                int intlAirports = Airports.GetAllAirports(a =>
                               a.Profile.Type == AirportProfile.AirportType.International).Count;

                foreach (Country country in this.SelectedCountries)
                {
                    count += Airports.GetAllAirports(a => (new CountryCurrentCountryConverter().Convert(a.Profile.Country) as Country) == country && (a.Profile.Type == AirportProfile.AirportType.Domestic)).Count;
                }

                this.NumberOfAirports = count + intlAirports;
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            foreach (Country country in this.SelectedCountries)
                this.StartData.SelectedCountries.Add(country);

            if (!this.StartData.RandomOpponents)
            {
                PageNavigator.NavigateTo(new PageSelectOpponents(this.StartData));
            }
            else
            {
                var scCreating = UIHelpers.FindChild<SplashControl>(this, "scCreating");

                if (scCreating != null)
                    scCreating.Visibility = Visibility.Visible;

                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (y, x) => { GameObjectHelpers.CreateGame(this.StartData); };
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
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

       

    }
}
