using TheAirline.Infrastructure;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageSettings.xaml
    /// </summary>
    public partial class PageSettings : Page
    {
        #region Constructors and Destructors

        public PageSettings()
        {
            this.InitializeComponent();

            this.Loaded += this.PageSettings_Loaded;
        }

        #endregion

        #region Public Properties

        public List<Language> AllLanguages
        {
            get
            {
                return Languages.GetLanguages().FindAll(l => l.IsEnabled);
            }
            private set
            {
                ;
            }
        }

        #endregion

        #region Methods

        private void PageSettings_Loaded(object sender, RoutedEventArgs e)
        {
            var rbFullScreen = UIHelpers.FindChild<RadioButton>(this, "rbFullScreen");
            rbFullScreen.IsChecked = Settings.GetInstance().Mode == Settings.ScreenMode.Fullscreen;

            var rbWindowed = UIHelpers.FindChild<RadioButton>(this, "rbWindowed");
            if (!rbFullScreen.IsChecked.Value)
            {
                rbWindowed.IsChecked = true;
            }

            var cbLanguage = UIHelpers.FindChild<ComboBox>(this, "cbLanguage");
            cbLanguage.SelectedItem = AppSettings.GetInstance().GetLanguage();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageStartMenu());
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var rbFullScreen = UIHelpers.FindChild<RadioButton>(this, "rbFullScreen");

            if (rbFullScreen.IsChecked.Value)
            {
                Settings.GetInstance().Mode = Settings.ScreenMode.Fullscreen;
                PageNavigator.MainWindow.WindowStyle = WindowStyle.None;
                PageNavigator.MainWindow.WindowState = WindowState.Maximized;
                PageNavigator.MainWindow.Focus();
            }
            else
            {
                Settings.GetInstance().Mode = Settings.ScreenMode.Windowed;
                PageNavigator.MainWindow.WindowStyle = WindowStyle.ToolWindow;
                PageNavigator.MainWindow.WindowState = WindowState.Maximized;
                PageNavigator.MainWindow.Focus();
            }

            var cbLanguage = UIHelpers.FindChild<ComboBox>(this, "cbLanguage");
            AppSettings.GetInstance().SetLanguage((Language)cbLanguage.SelectedItem);

            var file = new StreamWriter(AppSettings.GetCommonApplicationDataPath() + "\\game.settings");
            file.WriteLine(AppSettings.GetInstance().GetLanguage().Name);
            file.WriteLine(Settings.GetInstance().Mode);
            file.Close();

            //PageNavigator.NavigateTo(new PageStartMenu());
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            var rbFullScreen = UIHelpers.FindChild<RadioButton>(this, "rbFullScreen");
            rbFullScreen.IsChecked = Settings.GetInstance().Mode == Settings.ScreenMode.Fullscreen;

            var rbWindowed = UIHelpers.FindChild<RadioButton>(this, "rbWindowed");
            if (!rbFullScreen.IsChecked.Value)
            {
                rbWindowed.IsChecked = true;
            }

            var cbLanguage = UIHelpers.FindChild<ComboBox>(this, "cbLanguage");
            cbLanguage.SelectedItem = AppSettings.GetInstance().GetLanguage();
        }

        #endregion
    }
}