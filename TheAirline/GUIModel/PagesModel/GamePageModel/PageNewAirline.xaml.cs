using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
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

            txtCEO.Text = string.Format("{0} {1}", Names.GetInstance().getRandomFirstName(this.AllCountries[0]), Names.GetInstance().getRandomLastName(this.AllCountries[0]));
        }

        private void cbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Country country = (Country)cbCountry.SelectedItem;

            cbAirport.Items.Clear();

            foreach (Airport airport in Airports.GetAirports(country).OrderBy(a=>a.Profile.Name))
                cbAirport.Items.Add(airport);

   
        }
        private void btnCreateAndSave_Click(object sender, RoutedEventArgs e)
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
                        Airline nAirline = createAirline();
                        saveAirline(nAirline);
                    }
                }
                else
                {

                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2402"), Translator.GetInstance().GetString("MessageBox", "2402", "message"), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        Airline nAirline = createAirline();
                        saveAirline(nAirline);
                    }
                }

            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2404"), Translator.GetInstance().GetString("MessageBox", "2404", "message"), WPFMessageBoxButtons.Ok);

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
        //saves the airline
        private void saveAirline(Airline airline)
        {
            string directory = AppSettings.getCommonApplicationDataPath() + "\\custom airlines";
           
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string path = string.Format("{0}\\{1}.xml", directory,airline.Profile.Name);

            //saves the airline
            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            XmlElement root = xmlDoc.CreateElement("airline");
            xmlDoc.AppendChild(root);

            XmlElement profile = xmlDoc.CreateElement("profile");
            root.AppendChild(profile);

            profile.SetAttribute("name", airline.Profile.Name);
            profile.SetAttribute("iata", airline.Profile.IATACode);
            profile.SetAttribute("color", airline.Profile.Color);
            profile.SetAttribute("country", airline.Profile.Country.Uid);
            profile.SetAttribute("CEO", airline.Profile.CEO);
            profile.SetAttribute("mentality",airline.Mentality.ToString());
            profile.SetAttribute("market",airline.MarketFocus.ToString());
            profile.SetAttribute("preferedairport",airline.Profile.PreferedAirport != null ? airline.Profile.PreferedAirport.Profile.IATACode : "");
            profile.SetAttribute("routefocus",airline.AirlineRouteFocus.ToString());

            XmlElement info = xmlDoc.CreateElement("info");
            root.AppendChild(info);

            info.SetAttribute("real", false.ToString());
            info.SetAttribute("from", "1960");
            info.SetAttribute("to", "2199");

            xmlDoc.Save(path);

            //saves the logo
            string logopath = string.Format("{0}\\{1}.png", directory, airline.Profile.IATACode);

            // Construct a new image from the GIF file.
            System.Drawing.Bitmap logo = new System.Drawing.Bitmap(airline.Profile.Logo);
            logo.Save(logopath, System.Drawing.Imaging.ImageFormat.Png);

        }
        //creates the airline
        private Airline createAirline()
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

            return airline;
        }
    }
}
