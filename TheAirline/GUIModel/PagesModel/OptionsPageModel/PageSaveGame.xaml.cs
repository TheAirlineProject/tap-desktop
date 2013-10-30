using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
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
        private Boolean _isSaving;
        public Boolean IsSaving
        {
            get { return _isSaving; }
            set { _isSaving = value; NotifyPropertyChanged("IsSaving"); }
        }
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

            while (!GameObjectWorker.GetInstance().isCancelled())
            {
            }

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
                this.IsSaving = true;

                GameObject.GetInstance().Name = name;

                SerializedLoadSaveHelpers.SaveGame(name);
                
            }
          
            if (!gameworkerPaused)
                GameObjectWorker.GetInstance().start();

            this.IsSaving = false;
    
        }

        private void lbSaves_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtName.Text =  lbSaves.SelectedItem.ToString();
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
