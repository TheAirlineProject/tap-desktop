using System;
using System.Collections.Generic;
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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageSelectLanguage.xaml
    /// </summary>
    public partial class PageSelectLanguage : Page
    {
        public List<Language> AllLanguages { get; set; }
        public PageSelectLanguage()
        {
            this.AllLanguages = Languages.GetLanguages().FindAll(l => l.IsEnabled);

            InitializeComponent();
        }

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            Language language = (Language)((Hyperlink)sender).Tag;
            AppSettings.GetInstance().setLanguage(language);


            PageNavigator.NavigateTo(new PageStartMenu());
        }
    }
}
