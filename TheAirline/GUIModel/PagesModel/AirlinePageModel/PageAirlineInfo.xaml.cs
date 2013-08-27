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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineInfo.xaml
    /// </summary>
    public partial class PageAirlineInfo : Page
    {
        public AirlineMVVM Airline { get; set; }
        private string logoPath;
        public List<Airport> AllAirports { get; set; }
        public PageAirlineInfo(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;
            this.AllAirports = new List<Airport>();


            InitializeComponent();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvFleet.ItemsSource);
            view.GroupDescriptions.Clear();

            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Purchased");
            view.GroupDescriptions.Add(groupDescription);

            logoPath = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png";
            imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));

            foreach (Airport airport in this.Airline.Airline.Airports.FindAll(a => a.Terminals.getFreeSlotsPercent(this.Airline.Airline) > 50))
                this.AllAirports.Add(airport);

        }

        private void btnCreateSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            string iata = txtIATA.Text.ToUpper().Trim();
            string name = txtAirlineName.Text.Trim();
            Airport airport = (Airport)cbAirport.SelectedItem;
            string color = ((PropertyInfo)cbColor.SelectedItem).Name;
            Route.RouteType focus = rbPassengerType.IsChecked.Value ? Route.RouteType.Passenger : Route.RouteType.Cargo;
       
            string pattern = @"^[A-Za-z0-9]+$";
            Regex regex = new Regex(pattern);

            if (name.Length > 0 && iata.Length == 2 && regex.IsMatch(iata) && !Airlines.GetAllAirlines().Exists(a=>a.Profile.IATACode == iata))
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2402"), Translator.GetInstance().GetString("MessageBox", "2402", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    AirlineProfile profile = new AirlineProfile(name, iata, color, GameObject.GetInstance().MainAirline.Profile.CEO, false, GameObject.GetInstance().GameTime.Year, 2199);

                    profile.Logos.Clear();
                    profile.addLogo(new AirlineLogo(logoPath));
                    profile.Country = GameObject.GetInstance().MainAirline.Profile.Country;

                    SubsidiaryAirline subAirline = new SubsidiaryAirline(GameObject.GetInstance().MainAirline, profile, Model.AirlineModel.Airline.AirlineMentality.Safe, Model.AirlineModel.Airline.AirlineFocus.Local, Model.AirlineModel.Airline.AirlineLicense.Domestic, focus);
                    subAirline.addAirport(airport);
                 //   subAirline.Money = slMoney.Value;

                    this.Airline.addSubsidiaryAirline(subAirline);

                }


            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2404"), Translator.GetInstance().GetString("MessageBox", "2404", "message"), WPFMessageBoxButtons.Ok);


        }

        private void btnReleaseSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2118"), string.Format(Translator.GetInstance().GetString("MessageBox", "2118", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.MakeSubsidiaryAirlineIndependent(airline);

                    this.Airline.removeSubsidiaryAirline(airline);

                }
            }
        }

        private void btnDeleteSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2111"), string.Format(Translator.GetInstance().GetString("MessageBox", "2111", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.CloseSubsidiaryAirline(airline);

                    this.Airline.removeSubsidiaryAirline(airline);

                }
            }
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
        private void lnkAirliner_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageFleetAirliner(airliner));
        }
    }
}
