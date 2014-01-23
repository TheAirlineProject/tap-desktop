using System;
using System.Collections.Generic;
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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GUIModel.PagesModel.RoutesPageModel;
using TheAirline.GUIModel.PagesModel.AirlinersPageModel;
using System.ComponentModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirline.xaml
    /// </summary>
    public partial class PageAirline : Page, INotifyPropertyChanged
    {
        private AirlineMVVM Airline;
        public List<RouteProfitMVVM> ProfitRoutes { get; set; }
        public List<AirlineFleetSizeMVVM> MostUsedAircrafts { get; set; }
        public List<Airport> MostGates { get; set; }
        private Boolean _showactionmenu;
        public Boolean ShowActionMenu
        {
            get { return _showactionmenu; }
            set { _showactionmenu = value; NotifyPropertyChanged("ShowActionMenu"); }
        }
        private Boolean _hascodesharing;
        public Boolean HasCodesharing
        {
            get { return _hascodesharing; }
            set { _hascodesharing = value; NotifyPropertyChanged("HasCodesharing"); }
        }
        private Boolean _canhavealliance;
        public Boolean CanHaveAlliance
        {
            get { return _canhavealliance; }
            set { _canhavealliance = value; NotifyPropertyChanged("CanHaveAlliance"); }
        }
        public PageAirline(Airline airline)
        {
            this.Airline = new AirlineMVVM(airline);

            this.HasCodesharing = this.Airline.Airline.Codeshares.Exists(c => c.Airline1 == GameObject.GetInstance().HumanAirline || c.Airline2 == GameObject.GetInstance().HumanAirline);
            this.CanHaveAlliance = this.Airline.Airline.Alliances.Count == 0 && GameObject.GetInstance().HumanAirline.Alliances.Count > 0;
            this.ShowActionMenu = !this.Airline.Airline.IsHuman && (this.CanHaveAlliance || !this.HasCodesharing);

            var airports = this.Airline.Airline.Airports;

            var routes = this.Airline.Airline.Routes.OrderByDescending(r => r.Balance);

            double totalProfit = routes.Sum(r => r.Balance);

            this.ProfitRoutes = new List<RouteProfitMVVM>();
            foreach (Route route in routes.Take(Math.Min(5, routes.Count())))
            {
                this.ProfitRoutes.Add(new RouteProfitMVVM(route, totalProfit));
            }

            this.MostGates = airports.OrderByDescending(a => a.getAirlineContracts(this.Airline.Airline).Sum(c => c.NumberOfGates)).Take(Math.Min(5, airports.Count)).ToList();
            this.MostUsedAircrafts = new List<AirlineFleetSizeMVVM>();

            var types = this.Airline.Airline.Fleet.Select(a => a.Airliner.Type).Distinct();

            foreach (AirlinerType type in types)
            {
                int count = this.Airline.Airline.Fleet.Count(a => a.Airliner.Type == type);

                this.MostUsedAircrafts.Add(new AirlineFleetSizeMVVM(type, count));
            }

            this.MostUsedAircrafts = this.MostUsedAircrafts.OrderByDescending(a => a.Count).Take(Math.Min(5, this.MostUsedAircrafts.Count)).ToList();

            this.Loaded += PageAirline_Loaded;

            InitializeComponent();


        }

        private void PageAirline_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageAirlineInfo(this.Airline) { Tag = this });

            TabControl tab_main = UIHelpers.FindChild<TabControl>(this, "tcMenu");

            if (tab_main != null)
            {
                var matchingItem =
     tab_main.Items.Cast<TabItem>()
       .Where(item => item.Tag.ToString() == "Overview")
       .FirstOrDefault();

                //matchingItem.IsSelected = true;
                matchingItem.Header = this.Airline.Airline.Profile.Name;
                matchingItem.Visibility = System.Windows.Visibility.Visible;

                tab_main.SelectedItem = matchingItem;
            }

        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Overview" && frmContent != null)
                frmContent.Navigate(new PageAirlineInfo(this.Airline) { Tag = this });

            if (selection == "Finances" && frmContent != null)
                frmContent.Navigate(new PageAirlineFinances(this.Airline) { Tag = this });

            if (selection == "Employees" && frmContent != null)
                frmContent.Navigate(new PageAirlineEmployees(this.Airline) { Tag = this });

            if (selection == "Services" && frmContent != null)
                frmContent.Navigate(new PageAirlineServices(this.Airline) { Tag = this });

            if (selection == "Ratings" && frmContent != null)
                frmContent.Navigate(new PageAirlineRatings(this.Airline) { Tag = this });

            if (selection == "Insurances" && frmContent != null)
                frmContent.Navigate(new PageAirlineInsurance(this.Airline) { Tag = this });

            if (selection == "Fleet" && frmContent != null)
                frmContent.Navigate(new PageAirlineFleet(this.Airline) { Tag = this });
        }
        private void hlCodeSharing_Click(object sender, RoutedEventArgs e)
        {


            object o = PopUpCodeshareAgreement.ShowPopUp(this.Airline.Airline);

            if (o != null)
            {
                CodeshareAgreement.CodeshareType type = (CodeshareAgreement.CodeshareType)o;
                Boolean accepted = AirlineHelpers.AcceptCodesharing(this.Airline.Airline, GameObject.GetInstance().HumanAirline,type);

                if (accepted)
                {

                    this.HasCodesharing = true;
                    this.ShowActionMenu = !this.Airline.Airline.IsHuman && (this.CanHaveAlliance || !this.HasCodesharing);

                    CodeshareAgreement agreement = new CodeshareAgreement(this.Airline.Airline, GameObject.GetInstance().HumanAirline,type);

                    this.Airline.addCodeshareAgreement(agreement);
                    GameObject.GetInstance().HumanAirline.addCodeshareAgreement(agreement);

                }
                else
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2408"), string.Format(Translator.GetInstance().GetString("MessageBox", "2408", "message"), this.Airline.Airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }

        }
        private void hlAlliance_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbAlliances = new ComboBox();
            cbAlliances.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAlliances.SelectedValuePath = "Name";
            cbAlliances.DisplayMemberPath = "Name";
            cbAlliances.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAlliances.Width = 200;

            foreach (Alliance alliance in GameObject.GetInstance().HumanAirline.Alliances)
                cbAlliances.Items.Add(alliance);

            cbAlliances.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageAirline", "1005"), cbAlliances) == PopUpSingleElement.ButtonSelected.OK && cbAlliances.SelectedItem != null)
            {
                Alliance alliance = (Alliance)cbAlliances.SelectedItem;
                if (AIHelpers.DoAcceptAllianceInvitation(this.Airline.Airline, alliance))
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2605"), string.Format(Translator.GetInstance().GetString("MessageBox", "2605", "message"),this.Airline.Airline.Profile.Name,alliance.Name), WPFMessageBoxButtons.Ok);
                    alliance.addMember(new AllianceMember(this.Airline.Airline, GameObject.GetInstance().GameTime));

                    this.Airline.Alliance = alliance;

                    this.CanHaveAlliance = false;
                    this.ShowActionMenu = !this.Airline.Airline.IsHuman && (this.CanHaveAlliance || !this.HasCodesharing);

                }
                else
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2606"), string.Format(Translator.GetInstance().GetString("MessageBox", "2606", "message"), this.Airline.Airline.Profile.Name, alliance.Name), WPFMessageBoxButtons.Ok);

                }
            }
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
}
