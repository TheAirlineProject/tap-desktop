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
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.Airports;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpCompareAirports.xaml
    /// </summary>
    public partial class PopUpCompareAirports : PopUpWindow, INotifyPropertyChanged
    {
        public List<CompareAirportMVVM> Airports { get; set; }
        public ObservableCollection<CompareAirportMVVM> CompareAirports { get; set; }
        private CompareAirportMVVM _selectedairport;
        public CompareAirportMVVM SelectedAirport
        {
            get { return _selectedairport; }
            set { _selectedairport = value; NotifyPropertyChanged("SelectedAirport"); }
        }

        public static void ShowPopUp(List<Airport> airports)
        {
            PopUpWindow window = new PopUpCompareAirports(airports); ;
            window.ShowDialog();


        }
        public PopUpCompareAirports(List<Airport> airports)
        {
            this.Airports = new List<CompareAirportMVVM>();

            airports.ForEach(a=>this.Airports.Add(new CompareAirportMVVM(a)));

            this.CompareAirports = new ObservableCollection<CompareAirportMVVM>();

            this.DataContext = this.Airports;

            InitializeComponent();

        }

        private void cbAirport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CompareAirportMVVM airport = cbAirport.SelectedItem as CompareAirportMVVM;
            this.SelectedAirport = airport;

            var tAirports = new List<CompareAirportMVVM>(this.CompareAirports);

            foreach (CompareAirportMVVM a in tAirports)
                this.CompareAirports.Remove(a);

            foreach (CompareAirportMVVM a in this.Airports.Where(_ => _ != this.SelectedAirport))
                this.CompareAirports.Add(a);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
      
    }
    public class CompareAirportMVVM
    {
        public Airport Airport { get; set; }
        public int Routes { get; set; }
        public int ServingAirlines { get; set; }
        public CompareAirportMVVM(Airport airport)
        {
            this.Airport = airport;
            this.Routes = AirportHelpers.GetAirportRoutes(this.Airport).Count;
            this.ServingAirlines = this.Airport.AirlineContracts.Select(c => c.Airline).Distinct().Count();
        }
    }
    //the converter for the distance to the selected airport
    public class DistanceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CompareAirportMVVM airport1 = (CompareAirportMVVM)values[0];
            CompareAirportMVVM airport2 = (CompareAirportMVVM)values[1];

            double distance = MathHelpers.GetDistance(airport1.Airport, airport2.Airport);

            return new DistanceToUnitConverter().Convert(distance);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
