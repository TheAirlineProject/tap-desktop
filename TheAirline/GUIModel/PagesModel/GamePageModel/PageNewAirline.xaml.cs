namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml;

    using Microsoft.Win32;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Interaction logic for PageNewAirline.xaml
    /// </summary>
    public partial class PageNewAirline : Page
    {
        #region Fields

        private string logoPath;

        #endregion

        #region Constructors and Destructors

        public PageNewAirline()
        {
            this.Colors = new List<PropertyInfo>();
            this.AllCountries = new ObservableCollection<Country>(Countries.GetCountries().OrderBy(c => c.Name));

            foreach (PropertyInfo c in typeof(Colors).GetProperties())
            {
                this.Colors.Add(c);
            }

            this.InitializeComponent();

            this.logoPath = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png";
            this.imgLogo.Source = new BitmapImage(new Uri(this.logoPath, UriKind.RelativeOrAbsolute));

            this.txtCEO.Text = string.Format(
                "{0} {1}",
                Names.GetInstance().getRandomFirstName(this.AllCountries[0]),
                Names.GetInstance().getRandomLastName(this.AllCountries[0]));
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Country> AllCountries { get; set; }

        public List<PropertyInfo> Colors { get; set; }

        #endregion

        #region Methods

        private void btnCreateAirline_Click(object sender, RoutedEventArgs e)
        {
            string name = this.txtName.Text.Trim();
            string iata = this.txtIATA.Text.Trim().ToUpper();

            string pattern = @"^[A-Za-z0-9]+$";
            var regex = new Regex(pattern);

            if (name.Length > 0 && iata.Length == 2 && regex.IsMatch(iata))
            {
                Airline airline = Airlines.GetAirline(iata);

                if (airline != null)
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2401"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2401", "message"),
                                airline.Profile.Name),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        this.createAirline();
                    }
                }
                else
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2402"),
                            Translator.GetInstance().GetString("MessageBox", "2402", "message"),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        this.createAirline();
                    }
                }
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2404"),
                    Translator.GetInstance().GetString("MessageBox", "2404", "message"),
                    WPFMessageBoxButtons.Ok);
            }
        }

        private void btnCreateAndSave_Click(object sender, RoutedEventArgs e)
        {
            string name = this.txtName.Text.Trim();
            string iata = this.txtIATA.Text.Trim().ToUpper();

            string pattern = @"^[A-Za-z0-9]+$";
            var regex = new Regex(pattern);

            if (name.Length > 0 && iata.Length == 2 && regex.IsMatch(iata))
            {
                Airline airline = Airlines.GetAirline(iata);

                if (airline != null)
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2401"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2401", "message"),
                                airline.Profile.Name),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        Airline nAirline = this.createAirline();
                        this.saveAirline(nAirline);
                    }
                }
                else
                {
                    WPFMessageBoxResult result =
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2402"),
                            Translator.GetInstance().GetString("MessageBox", "2402", "message"),
                            WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                        Airline nAirline = this.createAirline();
                        this.saveAirline(nAirline);
                    }
                }
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2404"),
                    Translator.GetInstance().GetString("MessageBox", "2404", "message"),
                    WPFMessageBoxButtons.Ok);
            }
        }

        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.png)|*.png";
            dlg.InitialDirectory = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                this.logoPath = dlg.FileName;
                this.imgLogo.Source = new BitmapImage(new Uri(this.logoPath, UriKind.RelativeOrAbsolute));
            }
        }

        private void cbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var country = (Country)this.cbCountry.SelectedItem;

            this.cbAirport.Items.Clear();

            foreach (Airport airport in Airports.GetAirports(country).OrderBy(a => a.Profile.Name))
            {
                this.cbAirport.Items.Add(airport);
            }
        }

        //saves the airline

        //creates the airline
        private Airline createAirline()
        {
            string name = this.txtName.Text.Trim();
            string iata = this.txtIATA.Text.Trim().ToUpper();
            string ceo = this.txtCEO.Text.Trim();

            Airline tAirline = Airlines.GetAirline(iata);

            if (tAirline != null)
            {
                Airlines.RemoveAirline(tAirline);
            }

            var country = (Country)this.cbCountry.SelectedItem;
            string color = "DarkBlue";
                
            if (this.cbColor.SelectedItem != null)
                color = ((PropertyInfo)this.cbColor.SelectedItem).Name;

            var profile = new AirlineProfile(name, iata, color, ceo, false, 1950, 2199);
            profile.Countries = new List<Country> { country };
            profile.Country = country;
            profile.AddLogo(new AirlineLogo(this.logoPath));
            profile.PreferedAirports.Add(new DateTime(1900, 1, 1), this.cbAirport.SelectedItem != null ? (Airport)this.cbAirport.SelectedItem : null);

            Route.RouteType focus = this.rbPassengerType.IsChecked.Value
                ? Route.RouteType.Passenger
                : Route.RouteType.Cargo;

            var airline = new Airline(
                profile,
                Airline.AirlineMentality.Aggressive,
                Airline.AirlineFocus.Local,
                Airline.AirlineLicense.Domestic,
                focus,
                Airline.AirlineRouteSchedule.Regular);

            Airlines.AddAirline(airline);

            WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2405"),
                Translator.GetInstance().GetString("MessageBox", "2405", "message"),
                WPFMessageBoxButtons.Ok);

            return airline;
        }

        private void saveAirline(Airline airline)
        {
            string directory = AppSettings.getCommonApplicationDataPath() + "\\custom airlines";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string path = string.Format("{0}\\{1}.xml", directory, airline.Profile.Name);

            //saves the airline
            var xmlDoc = new XmlDocument();

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
            profile.SetAttribute("mentality", airline.Mentality.ToString());
            profile.SetAttribute("market", airline.MarketFocus.ToString());
            profile.SetAttribute(
                "preferedairport",
                airline.Profile.PreferedAirport != null ? airline.Profile.PreferedAirport.Profile.IATACode : "");
            profile.SetAttribute("routefocus", airline.AirlineRouteFocus.ToString());

            XmlElement info = xmlDoc.CreateElement("info");
            root.AppendChild(info);

            info.SetAttribute("real", false.ToString());
            info.SetAttribute("from", "1960");
            info.SetAttribute("to", "2199");

            xmlDoc.Save(path);

            //saves the logo
            string logopath = string.Format("{0}\\{1}.png", directory, airline.Profile.IATACode);

            // Construct a new image from the GIF file.
            var logo = new Bitmap(airline.Profile.Logo);
            logo.Save(logopath, ImageFormat.Png);
        }

        #endregion
    }
}