namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.CustomControlsModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;
    using TheAirline.Model.GeneralModel.Helpers.WorkersModel;

    /// <summary>
    ///     Interaction logic for PageSaveGame.xaml
    /// </summary>
    public partial class PageSaveGame : Page, INotifyPropertyChanged
    {
        #region Constructors and Destructors

        public PageSaveGame()
        {
            this.Saves = new ObservableCollection<string>();
            this.InitializeComponent();

            foreach (string savedFile in LoadSaveHelpers.GetSavedGames())
            {
                this.Saves.Add(savedFile);
            }
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<string> Saves { get; set; }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void btnSaveGame_Click(object sender, RoutedEventArgs e)
        {
            Boolean gameworkerPaused = GameObjectWorker.GetInstance().isPaused();

            GameObjectWorker.GetInstance().Cancel();

            string name = this.txtName.Text.Trim();

            Boolean doSave = true;

            if (SerializedLoadSaveHelpers.SaveGameExists(name))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "1007"),
                    Translator.GetInstance().GetString("MessageBox", "1007", "message"),
                    WPFMessageBoxButtons.YesNo);

                doSave = result == WPFMessageBoxResult.Yes;

                if (doSave)
                {
                    SerializedLoadSaveHelpers.DeleteSavedGame(name);
                }
            }

            if (doSave)
            {
                var scSaving = UIHelpers.FindChild<SplashControl>(this, "scSaving");

                scSaving.Visibility = Visibility.Visible;

                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (s, x) =>
                {
                    GameObject.GetInstance().Name = name;

                    SerializedLoadSaveHelpers.SaveGame(name);
                };
                bgWorker.RunWorkerCompleted += (s, x) =>
                {
                    if (!gameworkerPaused)
                    {
                        GameObjectWorker.GetInstance().Start();
                    }

                    scSaving.Visibility = Visibility.Collapsed;
                };
                bgWorker.RunWorkerAsync();
            }
        }

        private void lbSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.txtName.Text = this.lbSaves.SelectedItem.ToString();
        }

        #endregion
    }
}