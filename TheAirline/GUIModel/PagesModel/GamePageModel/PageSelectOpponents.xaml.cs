namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
    using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.MasterPageModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageSelectOpponents.xaml
    /// </summary>
    public partial class PageSelectOpponents : Page
    {
        private SplashControl scCreating;
        #region Constructors and Destructors

        public PageSelectOpponents(StartDataObject sdo)
        {
            this.StartData = sdo;

            this.SelectedAirlines = new ObservableCollection<Airline>();
            this.Opponents = new ObservableCollection<Airline>();

            this.Opponents = new ObservableCollection<Airline>(Airlines.GetAirlines(
                        a =>
                            a.Profile.Founded <= this.StartData.Year && a.Profile.Folded > this.StartData.Year
                            && a != this.StartData.Airline
                            && (a.Profile.Country.Region == this.StartData.Region
                                || (this.StartData.Continent != null
                                    && (this.StartData.Continent.Uid == "100"
                                        || this.StartData.Continent.hasRegion(a.Profile.Country.Region))))
                                        && ((this.StartData.MajorAirlines && a.MarketFocus == Airline.AirlineFocus.Global) || !this.StartData.MajorAirlines)).OrderBy(a => a.Profile.Name));
           
        
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Airline> Opponents { get; set; }

        public ObservableCollection<Airline> SelectedAirlines { get; set; }

        public StartDataObject StartData { get; set; }

        #endregion

        #region Methods
        private void SplashControl_Loaded(object sender, RoutedEventArgs e)
        {
            scCreating = (SplashControl)sender;
        }
        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            var airlines = new List<Airline>();

            foreach (Airline airline in this.SelectedAirlines)
            {
                airlines.Add(airline);
            }

            this.StartData.Opponents = airlines;

            scCreating.Visibility = System.Windows.Visibility.Visible;

            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (y, x) => { GameObjectHelpers.CreateGame(this.StartData); };
            bgWorker.RunWorkerCompleted += (y, x) =>
            {
                scCreating.Visibility = System.Windows.Visibility.Collapsed;

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                PageNavigator.ClearNavigator();
            };
            bgWorker.RunWorkerAsync();
        }

        private void imgDeselect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var airline = (Airline)((Image)sender).Tag;

            this.SelectedAirlines.Remove(airline);
            this.Opponents.Add(airline);
        }

        private void imgSelect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.SelectedAirlines.Count < this.StartData.NumberOfOpponents)
            {
                var airline = (Airline)((Image)sender).Tag;

                this.SelectedAirlines.Add(airline);
                this.Opponents.Remove(airline);
            }
        }

        #endregion
    }
}