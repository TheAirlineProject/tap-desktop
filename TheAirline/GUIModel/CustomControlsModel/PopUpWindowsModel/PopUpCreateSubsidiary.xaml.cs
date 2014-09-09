using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.RouteModel;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpCreateSubsidiary.xaml
    /// </summary>
    public partial class PopUpCreateSubsidiary : PopUpWindow, INotifyPropertyChanged
    {
        private string logoPath;
        public Airline Airline { get; set; }
        public ObservableCollection<Airport> AllAirports { get; set; }
        public List<PropertyInfo> Colors { get; set; }
        public double MaxSubsidiaryMoney { get; set; }
        private Boolean _iataok;
        public Boolean IATAOk
        {
            get
            {
                return this._iataok;
            }
            set
            {
                this._iataok = value;
                this.NotifyPropertyChanged("IATAOk");
            }
        }
        public static object ShowPopUp(Airline airline)
        {
            PopUpWindow window = new PopUpCreateSubsidiary(airline);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpCreateSubsidiary(Airline airline)
        {
            this.Airline = airline;

            this.DataContext = this.Airline;

            this.MaxSubsidiaryMoney = this.Airline.Money / 2;

            this.IATAOk = false;

            this.Colors = new List<PropertyInfo>();

            foreach (PropertyInfo c in typeof(Colors).GetProperties())
            {
                this.Colors.Add(c);
            }

            this.AllAirports = new ObservableCollection<Airport>();
            /*
            foreach (
                Airport airport in
                    this.Airline.Airports.FindAll(
                    a => a.Terminals.getFreeSlotsPercent(this.Airline,this.Airline.AirlineRouteFocus == Route.RouteType.Passenger ? Terminal.TerminalType.Passenger : Terminal.TerminalType.Cargo) > 50))
            {
                this.AllAirports.Add(airport);
            }*/
            
            this.Loaded += PopUpCreateSubsidiary_Loaded;

            InitializeComponent();

        }

        private void PopUpCreateSubsidiary_Loaded(object sender, RoutedEventArgs e)
        {
            this.logoPath = AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\default.png";
            this.imgLogo.Source = new BitmapImage(new Uri(this.logoPath, UriKind.RelativeOrAbsolute));

        }
        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.png)|*.png";
            dlg.InitialDirectory = AppSettings.GetDataPath() + "\\graphics\\airlinelogos\\";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                this.logoPath = dlg.FileName;
                this.imgLogo.Source = new BitmapImage(new Uri(this.logoPath, UriKind.RelativeOrAbsolute));
            }
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            string iata = this.txtIATA.Text.ToUpper().Trim();
            string name = this.txtAirlineName.Text.Trim();
            var airport = (Airport)this.cbAirport.SelectedItem;
            string color = ((PropertyInfo)this.cbColor.SelectedItem).Name;
            Route.RouteType focus = this.rbPassengerType.IsChecked.Value
                ? Route.RouteType.Passenger
                : Route.RouteType.Cargo;

            var profile = new AirlineProfile(
                       name,
                       iata,
                       color,
                       GameObject.GetInstance().MainAirline.Profile.CEO,
                       false,
                       GameObject.GetInstance().GameTime.Year,
                       2199);

            profile.Country = airport.Profile.Country; //GameObject.GetInstance().MainAirline.Profile.Country;<

            var subAirline = new SubsidiaryAirline(
                GameObject.GetInstance().MainAirline,
                profile,
                Model.AirlineModel.Airline.AirlineMentality.Safe,
                Model.AirlineModel.Airline.AirlineFocus.Local,
                Model.AirlineModel.Airline.AirlineLicense.Domestic,
                focus);

            subAirline.AddAirport(airport);
            subAirline.Profile.Logos.Clear();
            subAirline.Profile.AddLogo(new AirlineLogo(this.logoPath));
            subAirline.Money = this.slMoney.Value;

            this.Selected = subAirline;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
        private void rbPassengerType_Checked(object sender, RoutedEventArgs e)
        {
            Route.RouteType focus = this.rbPassengerType.IsChecked.Value
          ? Route.RouteType.Passenger
          : Route.RouteType.Cargo;

            while (this.AllAirports.Count > 0)
            {
                this.AllAirports.RemoveAt(this.AllAirports.Count - 1);
            }

            foreach (
               Airport airport in
                   this.Airline.Airports.FindAll(
                   a => a.Terminals.GetFreeSlotsPercent(this.Airline, focus == Route.RouteType.Passenger ? Terminal.TerminalType.Passenger : Terminal.TerminalType.Cargo) > 50))
            {
                this.AllAirports.Add(airport);
            }

        }
        private void txtAirlineName_TextChanged(object sender, TextChangedEventArgs e)
        {
            setIATAOk();

        }

        private void txtIATA_TextChanged(object sender, TextChangedEventArgs e)
        {

            setIATAOk();

        }
        private void cbAirport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setIATAOk();
        }
        private void setIATAOk()
        {
            string iata = this.txtIATA.Text.ToUpper().Trim();
            string name = this.txtAirlineName.Text.Trim();

            string pattern = @"^[A-Za-z0-9]+$";
            var regex = new Regex(pattern);


            this.IATAOk = (name.Length > 0 && iata.Length == 2 && regex.IsMatch(iata)
              && !Airlines.GetAllAirlines().Exists(a => a.Profile.IATACode == iata) && cbAirport.SelectedItem != null);

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
