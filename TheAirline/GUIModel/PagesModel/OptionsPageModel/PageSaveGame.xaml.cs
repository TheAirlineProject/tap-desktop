using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    /// Interaction logic for PageSaveGame.xaml
    /// </summary>
    public partial class PageSaveGame : Page
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

        }
    }
}
