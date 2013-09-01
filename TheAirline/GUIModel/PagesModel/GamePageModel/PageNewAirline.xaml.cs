using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageNewAirline.xaml
    /// </summary>
    public partial class PageNewAirline : Page
    {
        public List<PropertyInfo> Colors { get; set; }
        public List<Country> AllCountries { get; set; }
        private string logoPath;
        public PageNewAirline()
        {
            this.Colors = new List<PropertyInfo>();
            this.AllCountries = Countries.GetCountries().OrderBy(c => c.Name).ToList();

            foreach (PropertyInfo c in typeof(Colors).GetProperties())
                this.Colors.Add(c);

            InitializeComponent();

            logoPath = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png";
            imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));

            txtCEO.Text = string.Format("{0} {1}", Names.GetInstance().getRandomFirstName(), Names.GetInstance().getRandomLastName());
        }

        private void cbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Country country = (Country)cbCountry.SelectedItem;

            cbAirport.Items.Clear();

            foreach (Airport airport in Airports.GetAirports(country).OrderBy(a=>a.Profile.Name))
                cbAirport.Items.Add(airport);
        }

        private void btnCreateAirline_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string iata = txtIATA.Text.Trim().ToUpper();
           
            string pattern = @"^[A-Za-z0-9]+$";
            Regex regex = new Regex(pattern);

            if (name.Length > 0 && iata.Length == 2 && regex.IsMatch(iata))
            {
                Airline airline = Airlines.GetAirline(iata);

                if (airline != null)
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2401"), string.Format(Translator.GetInstance().GetString("MessageBox", "2401", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        createAirline();
                    }
                }
                else
                {

                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2402"), Translator.GetInstance().GetString("MessageBox", "2402", "message"), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        createAirline();
                    }
                }

            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2404"), Translator.GetInstance().GetString("MessageBox", "2404", "message"), WPFMessageBoxButtons.Ok);
        }

        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.png)|*.png";
            dlg.InitialDirectory = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                logoPath = dlg.FileName;
                imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));
               
            }

        }
        private void createAirline()
        {
            string name = txtName.Text.Trim();
            string iata = txtIATA.Text.Trim().ToUpper();
            string ceo = txtCEO.Text.Trim();

            Airline tAirline = Airlines.GetAirline(iata);

            if (tAirline != null)
                Airlines.RemoveAirline(tAirline);

            Country country = (Country)cbCountry.SelectedItem;
            string color = ((PropertyInfo)cbColor.SelectedItem).Name;

            AirlineProfile profile = new AirlineProfile(name, iata, color, ceo, false, 1950, 2199);
            profile.Countries = new List<Country>() { country };
            profile.Country = country;
            profile.addLogo(new AirlineLogo(logoPath));
            profile.PreferedAirport = cbAirport.SelectedItem != null ? (Airport)cbAirport.SelectedItem : null;

            Route.RouteType focus = rbPassengerType.IsChecked.Value ? Route.RouteType.Passenger : Route.RouteType.Cargo;

            Airline airline = new Airline(profile, Airline.AirlineMentality.Aggressive, Airline.AirlineFocus.Local, Airline.AirlineLicense.Domestic, focus);

            Airlines.AddAirline(airline);

            WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2405"), Translator.GetInstance().GetString("MessageBox", "2405", "message"), WPFMessageBoxButtons.Ok);
   

        }
    }
}
