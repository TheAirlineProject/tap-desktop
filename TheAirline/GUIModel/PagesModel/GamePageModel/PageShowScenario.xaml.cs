using TheAirline.Helpers;
using TheAirline.Models.General.Scenarios;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;

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
            ScenarioHelpers.SetupScenario(this.Scenario);
        }

        private void cbScenario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Scenario = (Scenario)((ComboBox)sender).SelectedItem;
            this.DataContext = this.Scenario;
        }

        #endregion
    }
}