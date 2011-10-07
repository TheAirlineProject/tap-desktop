using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TheAirlineV2.Properties;

namespace TheAirlineV2.GraphicsModel.PageModel.GeneralModel
{
    public class PageTopMenu : Page
    {
        protected WrapPanel panelMain;
        public PageTopMenu()
        {
         //   Brush brush = new SolidColorBrush(Colors.Black);
          //  brush.Opacity = 0.80;

          //  this.Background = brush;

            this.SetResourceReference(Page.BackgroundProperty, "BackgroundTop");

            /*
            Border frameBorder = new Border();
            frameBorder.BorderBrush = Brushes.White;
            //frameBorder.CornerRadius = new CornerRadius(5);
            frameBorder.BorderThickness = new Thickness(2);
            */
            panelMain = new WrapPanel();
            panelMain.Margin = new Thickness(1, 0, 0, 0);
            panelMain.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
     
            TextBlock txtName = new TextBlock();
            txtName.FontSize = 16;
            txtName.FontWeight = FontWeights.Bold;
            txtName.Text = "The Airline " + Settings.Default.Version;
            //txtName.Foreground = Brushes.White;
            txtName.Margin = new Thickness(5, 0, 0, 0);
            //txtName.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            panelMain.Children.Add(txtName);

            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(@"/Data/images/Airplane-white.png", UriKind.RelativeOrAbsolute));
            RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);

            imgLogo.Margin = new Thickness(10, 0, 50, 0);


            panelMain.Children.Add(imgLogo);

            //frameBorder.Child = panelMain;

            this.Content = panelMain;

        }
    }
}
