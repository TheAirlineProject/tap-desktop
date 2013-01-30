using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TheAirline.Properties;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    public class PageTopMenu : Page
    {
        protected WrapPanel panelMain;
        public PageTopMenu()
        {
      
            this.SetResourceReference(Page.BackgroundProperty, "BackgroundTop");

           panelMain = new WrapPanel();
            panelMain.Margin = new Thickness(1, 0, 0, 0);
            panelMain.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
     
            TextBlock txtName = new TextBlock();
            txtName.FontSize = 16;
            txtName.FontWeight = FontWeights.Bold;
            txtName.Text = "Airline Project " + Settings.Default.Version;
            txtName.Margin = new Thickness(5, 0, 0, 0);
      
            panelMain.Children.Add(txtName);

            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(@"/Data/images/Airplane-white.png", UriKind.RelativeOrAbsolute));
            RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);

            imgLogo.Margin = new Thickness(10, 0, 50, 0);


            panelMain.Children.Add(imgLogo);

            this.Content = panelMain;

        }
    }
}
