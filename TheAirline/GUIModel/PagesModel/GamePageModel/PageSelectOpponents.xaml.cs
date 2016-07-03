using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.MasterPageModel;
using TheAirline.GUIModel.ObjectsModel;
using TheAirline.Helpers;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Views.Airline;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    ///     Interaction logic for PageSelectOpponents.xaml
    /// </summary>
    public partial class PageSelectOpponents : Page
    {
        #region Constructors and Destructors

        public PageSelectOpponents(StartDataObject sdo)
        {
            StartData = sdo;

            SelectedAirlines = new ObservableCollection<Airline>();
            Opponents = new ObservableCollection<Airline>();

            foreach (
                Airline airline in
                    Airlines.GetAirlines(
                        a =>
                            a.Profile.Founded <= StartData.Year && a.Profile.Folded > StartData.Year
                            && a != StartData.Airline
                            && (a.Profile.Country.Region == StartData.Region
                                || (StartData.Continent != null
                                    && (StartData.Continent.Uid == "100"
                                        || StartData.Continent.HasRegion(a.Profile.Country.Region))))))
            {
                Opponents.Add(airline);
            }

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Airline> Opponents { get; set; }

        public ObservableCollection<Airline> SelectedAirlines { get; set; }

        public StartDataObject StartData { get; set; }

        #endregion

        #region Methods

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            var airlines = new List<Airline>();

            foreach (Airline airline in SelectedAirlines)
            {
                airlines.Add(airline);
            }

            StartData.Opponents = airlines;

            var smp = Content as StandardMasterPage;

            // SplashControl scCreating = UIHelpers.FindChild<SplashControl>(smp, "scCreating"); 

            //scCreating.Visibility = System.Windows.Visibility.Visible;

            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (y, x) => { GameObjectHelpers.CreateGame(StartData); };
            bgWorker.RunWorkerCompleted += (y, x) =>
            {
                //  scCreating.Visibility = System.Windows.Visibility.Collapsed;

                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                PageNavigator.ClearNavigator();
            };
            bgWorker.RunWorkerAsync();
        }

        private void imgDeselect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var airline = (Airline)((Image)sender).Tag;

            SelectedAirlines.Remove(airline);
            Opponents.Add(airline);
        }

        private void imgSelect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedAirlines.Count < StartData.NumberOfOpponents)
            {
                var airline = (Airline)((Image)sender).Tag;

                SelectedAirlines.Add(airline);
                Opponents.Remove(airline);
            }
        }

        #endregion
    }
}