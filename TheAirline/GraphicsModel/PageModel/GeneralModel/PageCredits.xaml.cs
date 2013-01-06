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
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    /// <summary>
    /// Interaction logic for PageCredits.xaml
    /// </summary>
    public partial class PageCredits : StandardPage
    {
        public PageCredits()
        {
            InitializeComponent();

            StackPanel panelContent = new StackPanel();
            panelContent.Margin = new Thickness(10, 0, 10, 0);
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;


            Image imgCredits = new Image();
            imgCredits.Source = new BitmapImage(new Uri(AppSettings.getDataPath() + "\\graphics\\credits.png", UriKind.RelativeOrAbsolute));
            imgCredits.Height = GraphicsHelpers.GetContentHeight();
            RenderOptions.SetBitmapScalingMode(imgCredits, BitmapScalingMode.HighQuality);
            

            panelContent.Children.Add(imgCredits);

            //base.setTopMenu(new PageTopMenu());

            //base.hideNavigator();

            //base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent(Translator.GetInstance().GetString("PageCredits","1000"));

            showPage(this);

        }
    }
}
