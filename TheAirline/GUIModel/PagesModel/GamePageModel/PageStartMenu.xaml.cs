namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageStartMenu.xaml
    /// </summary>
    public partial class PageStartMenu : Page
    {
        #region Constructors and Destructors

        public PageStartMenu()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void btnCredits_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageCredits());
            //PageNavigator.NavigateTo(new PageSelectAirports());
        }

        private void btnExitGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "1003"),
                Translator.GetInstance().GetString("MessageBox", "1003", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                PageNavigator.MainWindow.Close();
            }
        }

        private void btnLoadGame_Click(object sender, RoutedEventArgs e)
        {
            /*
            //WPFPopUpLoadConfiguration conf = new WPFPopUpLoadConfiguration(Configuration.ConfigurationType.Airliner);
            
            DataTemplate template = this.Resources["TestTemplate"] as DataTemplate;

             WPFRegularPopUp.Show("Test",template,WPFPopUpButtons.YesNo);

   
             DependencyObject o= template.LoadContent();

             FrameworkElementFactory ff= template.VisualTree;
             ComboBox cb = UIHelpers.FindChild<ComboBox>(o, "cbTest");
            */

            PageNavigator.NavigateTo(new PageMMLoadGame());
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewGame());
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageSettings());
        }

        #endregion
    }
}