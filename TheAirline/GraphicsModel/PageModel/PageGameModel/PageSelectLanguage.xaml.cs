using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageGameModel
{
    /// <summary>
    /// Interaction logic for PageSelectLanguage.xaml
    /// </summary>
    public partial class PageSelectLanguage : StandardPage
    {
        public PageSelectLanguage()
        {
            InitializeComponent();

            StackPanel panelContent = new StackPanel();
            panelContent.Margin = new Thickness(10, 0, 10, 0);
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 100, 0, 0);

            panelContent.Children.Add(panelButtons);

            foreach (Language language in Languages.GetLanguages().FindAll(l => l.IsEnabled))
            {
                ContentControl ccLanguage = new ContentControl();
                ccLanguage.ContentTemplate = this.Resources["LanguageItem"] as DataTemplate;
                ccLanguage.Content = language;
                ccLanguage.Margin = new Thickness(0, 0, 20, 0);

                panelButtons.Children.Add(ccLanguage);
            }

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent("Select language");

            showPage(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Language language = (Language)((Button)sender).Tag;
            AppSettings.GetInstance().setLanguage(language);
          

            PageNavigator.NavigateTo(new PageFrontMenu());
        }
    }
}
