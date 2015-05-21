using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Helpers.Workers;
using TheAirline.Infrastructure;
using TheAirline.Models.General;
using TheAirline.Models.General.Holidays;
using TheAirline.Views.Airline;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    ///     Interaction logic for PageLoadGame.xaml
    /// </summary>
    public partial class PageLoadGame : Page
    {
        #region Constructors and Destructors

        public PageLoadGame()
        {
            Saves = new ObservableCollection<string>();

            InitializeComponent();

            foreach (string savedFile in LoadSaveHelpers.GetSavedGames())
            {
                Saves.Add(savedFile);
            }
        }

        #endregion

        #region Public Properties

        public ObservableCollection<string> Saves { get; set; }

        #endregion

        #region Methods

        private void btnDeleteGame_Click(object sender, RoutedEventArgs e)
        {
            var file = (string)lbSaves.SelectedItem;
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "1009"),
                Translator.GetInstance().GetString("MessageBox", "1009", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                SerializedLoadSaveHelpers.DeleteSavedGame(file);

                Saves.Remove(file);
            }
        }

        private void btnLoadGame_Click(object sender, RoutedEventArgs e)
        {
            GameObjectWorker.GetInstance().Cancel();

            while (GameObjectWorker.GetInstance().IsBusy())
            {
            }

            var file = (string)lbSaves.SelectedItem;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "1002"),
                Translator.GetInstance().GetString("MessageBox", "1002", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                if (file != null)
                {
                    var scLoading = UIHelpers.FindChild<SplashControl>(this, "scLoading");

                    scLoading.Visibility = Visibility.Visible;

                    var bgWorker = new BackgroundWorker();
                    bgWorker.DoWork += (s, x) => { SerializedLoadSaveHelpers.LoadGame(file); };
                    bgWorker.RunWorkerCompleted += (s, x) =>
                    {
                        scLoading.Visibility = Visibility.Collapsed;

                        HolidayYear.Clear();

                        GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

                        Setup.SetupMergers();

                        GameObjectWorker.GetInstance().Pause();

                        PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
                    };
                    bgWorker.RunWorkerAsync();
                }
            }
        }

        #endregion
    }
}