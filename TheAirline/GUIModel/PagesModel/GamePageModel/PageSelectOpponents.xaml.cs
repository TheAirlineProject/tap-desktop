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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageSelectOpponents.xaml
    /// </summary>
    public partial class PageSelectOpponents : Page
    {
        public StartDataObject StartData { get; set; }
        public ObservableCollection<Airline> SelectedAirlines { get; set; }
        public ObservableCollection<Airline> Opponents { get; set; }
        public PageSelectOpponents(StartDataObject sdo)
        {
            this.StartData = sdo;

            this.SelectedAirlines = new ObservableCollection<Airline>();
            this.Opponents = new ObservableCollection<Airline>();
         
            foreach (Airline airline in Airlines.GetAirlines(a => a.Profile.Founded <= this.StartData.Year && a.Profile.Folded > this.StartData.Year && a != this.StartData.Airline && (a.Profile.Country.Region == this.StartData.Region || (this.StartData.Continent != null && (this.StartData.Continent.Uid == "100" || this.StartData.Continent.hasRegion(a.Profile.Country.Region))))))
                this.Opponents.Add(airline);

            InitializeComponent();
        }

        private void imgSelect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.SelectedAirlines.Count < this.StartData.NumberOfOpponents)
            {
                Airline airline = (Airline)((Image)sender).Tag;

                this.SelectedAirlines.Add(airline);
                this.Opponents.Remove(airline);
            }
        }

        private void imgDeselect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Airline airline = (Airline)((Image)sender).Tag;

            this.SelectedAirlines.Remove(airline);
            this.Opponents.Add(airline);
       
        }

        private void btnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            List<Airline> airlines = new List<Airline>();

            foreach (Airline airline in this.SelectedAirlines)
                airlines.Add(airline);

            this.StartData.Opponents = airlines;

         
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (y, x) =>
            {
                GameObjectHelpers.CreateGame(this.StartData);

            };
            bgWorker.RunWorkerCompleted += (y, x) =>
            {
            
                PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                PageNavigator.ClearNavigator();
       

            };
            bgWorker.RunWorkerAsync();

        
          
        }
    }
}
