using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    /// Interaction logic for PageSaveGame.xaml
    /// </summary>
    public partial class PageSaveGame : Page, INotifyPropertyChanged
    {
        public ObservableCollection<string> Saves { get; set; }

        public PageSaveGame()
        {
            this.Saves = new ObservableCollection<string>();
            InitializeComponent();

            foreach (string savedFile in LoadSaveHelpers.GetSavedGames())
                this.Saves.Add(savedFile);

        }

        private void btnSaveGame_Click(object sender, RoutedEventArgs e)
        {
            Boolean gameworkerPaused = GameObjectWorker.GetInstance().isPaused();

            GameObjectWorker.GetInstance().cancel();

            string name = txtName.Text.Trim();

            Boolean doSave = true;

           
                if (SerializedLoadSaveHelpers.SaveGameExists(name))
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1007"), Translator.GetInstance().GetString("MessageBox", "1007", "message"), WPFMessageBoxButtons.YesNo);

                    doSave = result == WPFMessageBoxResult.Yes;

                    if (doSave)
                    {
                        SerializedLoadSaveHelpers.DeleteSavedGame(name);
                    }
                }

                if (doSave)
                {
                    SplashControl scSaving = UIHelpers.FindChild<SplashControl>(this, "scSaving");

                    scSaving.Visibility = System.Windows.Visibility.Visible;

                    BackgroundWorker bgWorker = new BackgroundWorker();
                    bgWorker.DoWork += (s, x) =>
                    {
                        GameObject.GetInstance().Name = name;

                        SerializedLoadSaveHelpers.SaveGame(name);



                    };
                    bgWorker.RunWorkerCompleted += (s, x) =>
                    {
                        if (!gameworkerPaused)
                            GameObjectWorker.GetInstance().start();

                        scSaving.Visibility = System.Windows.Visibility.Collapsed;
                    };
                    bgWorker.RunWorkerAsync();

                

            }
        }
        private void lbSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtName.Text = lbSaves.SelectedItem.ToString();
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
