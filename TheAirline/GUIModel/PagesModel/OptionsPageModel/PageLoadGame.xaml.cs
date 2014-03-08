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
using TheAirline.GUIModel.CustomControlsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using TheAirline.Model.GeneralModel.HolidaysModel;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    /// Interaction logic for PageLoadGame.xaml
    /// </summary>
    public partial class PageLoadGame : Page
    {
        public ObservableCollection<string> Saves { get; set; }
      
        public PageLoadGame()
        {
            this.Saves = new ObservableCollection<string>();

            InitializeComponent();
         
            foreach (string savedFile in LoadSaveHelpers.GetSavedGames())
                this.Saves.Add(savedFile);

            
        }
        private void btnDeleteGame_Click(object sender, RoutedEventArgs e)
        {
               string file = (string)lbSaves.SelectedItem;
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1009"), Translator.GetInstance().GetString("MessageBox", "1009", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                SerializedLoadSaveHelpers.DeleteSavedGame(file);

                Saves.Remove(file);
            }
        }
        private void btnLoadGame_Click(object sender, RoutedEventArgs e)
        {
           
            GameObjectWorker.GetInstance().cancel();

            while (GameObjectWorker.GetInstance().isBusy())
            {
            }

            string file = (string)lbSaves.SelectedItem;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "1002"), Translator.GetInstance().GetString("MessageBox", "1002", "message"), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
             
                if (file != null)
                {
                   
                    SplashControl scLoading = UIHelpers.FindChild<SplashControl>(this, "scLoading");

                    scLoading.Visibility = System.Windows.Visibility.Visible;
               
                    BackgroundWorker bgWorker = new BackgroundWorker();
                    bgWorker.DoWork += (s, x) =>
                    {
                        SerializedLoadSaveHelpers.LoadGame(file);

                    };
                    bgWorker.RunWorkerCompleted += (s, x) =>
                    {
                        scLoading.Visibility = System.Windows.Visibility.Collapsed;

                        HolidayYear.Clear();

                        GeneralHelpers.CreateHolidays(GameObject.GetInstance().GameTime.Year);

                        Setup.SetupMergers();

                        GameObjectWorker.GetInstance().pause();

                        PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));

                     };
                    bgWorker.RunWorkerAsync();
                    
              

                 


                }

            }

     
        }
    }
}
