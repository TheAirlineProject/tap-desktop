using System.ComponentModel.Composition;
using System.Windows;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.GamePageModel;
using TheAirline.Models.General;
using TheAirline.ViewModels.Game;

namespace TheAirline.Views.Game
{
    /// <summary>
    ///     Interaction logic for PageStartMenu.xaml
    /// </summary>
    [Export("PageStartMenu")]
    public partial class PageStartMenu
    {
        #region Constructors and Destructors

        public PageStartMenu()
        {
            InitializeComponent();
        }

        #endregion

        [Import]
        public PageStartMenuViewModel ViewModel
        {
            get { return DataContext as PageStartMenuViewModel; }
            set { DataContext = value; }
        }

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