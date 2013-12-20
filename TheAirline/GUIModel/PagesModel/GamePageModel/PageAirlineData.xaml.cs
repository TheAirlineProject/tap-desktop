using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GUIModel.CustomControlsModel;
using System.ComponentModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineData.xaml
    /// </summary>
    public partial class PageAirlineData : Page
    {
        public List<GameTimeZone> AllTimeZones { get; set; }
        public ObservableCollection<Airport> AllAirports { get; set; }
        private StartDataObject StartData;
        public PageAirlineData(StartDataObject startData)
        {
            this.AllTimeZones = TimeZones.GetTimeZones();
            this.AllAirports = new ObservableCollection<Airport>();
            this.StartData = startData;
            
            InitializeComponent();

            var airlines = Airlines.GetAirlines(airline => (airline.Profile.Country.Region == this.StartData.Region || (this.StartData.Region.Uid == "100" && this.StartData.Continent.Uid == "100") || (this.StartData.Region.Uid == "100" && this.StartData.Continent.hasRegion(airline.Profile.Country.Region))) && airline.Profile.Founded <= this.StartData.Year && airline.Profile.Folded > this.StartData.Year).OrderBy(a=>a.Profile.Name).ToList();

            cbAirline.ItemsSource = airlines;

              
        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Airline airline = (Airline)cbAirline.SelectedItem;

            this.AllAirports.Clear();

            foreach (var airport in Airports.GetAllAirports(a => airline.Profile.Countries.Contains(a.Profile.Country)).OrderBy(a=>a.Profile.Name))
                this.AllAirports.Add(airport);

            if (this.AllAirports.Contains(airline.Profile.PreferedAirport))
                cbAirport.SelectedItem = airline.Profile.PreferedAirport;
            else
                cbAirport.SelectedIndex = 0;
        }
        private void btnStartMenu_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }
        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            this.StartData.Airline = (Airline)cbAirline.SelectedItem;
            this.StartData.Airport = (Airport)cbAirport.SelectedItem;
            this.StartData.CEO = (string)txtCEO.Text;
            this.StartData.HomeCountry = (Country)cbCountry.SelectedItem;
            this.StartData.TimeZone = (GameTimeZone)cbTimeZone.SelectedItem;
            this.StartData.LocalCurrency = cbLocalCurrency.IsChecked.Value && this.StartData.HomeCountry.HasLocalCurrency;
         
            if (!this.StartData.RandomOpponents)
            {
                PageNavigator.NavigateTo(new PageSelectOpponents(this.StartData));
            }
            else
            {
                SplashControl scCreating = UIHelpers.FindChild<SplashControl>(this, "scCreating");

                scCreating.Visibility = System.Windows.Visibility.Visible;

                BackgroundWorker bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (y, x) =>
                {
                    GameObjectHelpers.CreateGame(this.StartData);

                };
                bgWorker.RunWorkerCompleted += (y, x) =>
                {
                    scCreating.Visibility = System.Windows.Visibility.Collapsed;

                    PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                    PageNavigator.ClearNavigator();

                };
                bgWorker.RunWorkerAsync();

            
            }

        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>((Page)this.Tag, "frmContent");

            frmContent.Navigate(new PageStartData() { Tag = this.Tag });
        }
       
    }
}
