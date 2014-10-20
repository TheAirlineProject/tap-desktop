namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GUIModel.CustomControlsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.ScenarioModel;

    /// <summary>
    ///     Interaction logic for PageShowScenario.xaml
    /// </summary>
    public partial class PageShowScenario : Page
    {
        #region Constructors and Destructors

        public PageShowScenario()
        {
            this.AllScenarios = Scenarios.GetScenarios();

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<Scenario> AllScenarios { get; set; }

        public Scenario Scenario { get; set; }

        #endregion

        #region Methods

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            SplashControl scCreating = UIHelpers.FindChild<SplashControl>(this, "scCreating"); 

            scCreating.Visibility = System.Windows.Visibility.Visible;

            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (y, x) => { ScenarioHelpers.SetupScenario(this.Scenario); ; };
            bgWorker.RunWorkerCompleted += (y, x) =>
            {
                 scCreating.Visibility = System.Windows.Visibility.Collapsed;

                   PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

            PageNavigator.ClearNavigator();

            };
            bgWorker.RunWorkerAsync();
           
        }

        private void cbScenario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Scenario = (Scenario)((ComboBox)sender).SelectedItem;
            this.DataContext = this.Scenario;
        }

        #endregion
    }
}