using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    public class PageHeader : Page
    {
        private TextBlock txtText;
        public string Text { get { return txtText.Text; } set { txtText.Text = value; } }
        public PageHeader()
        {
            Brush brush = new SolidColorBrush(Colors.DarkGray);
            brush.Opacity = 0.60;

            this.Background = brush;

            Border frameBorder = new Border();
            frameBorder.BorderBrush = Brushes.Gray;
            frameBorder.BorderThickness = new Thickness(2);

            DockPanel panelMain = new DockPanel();
            panelMain.Margin = new Thickness(5, 5, 5, 5);
            panelMain.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

          

            txtText = new TextBlock();
            txtText.FontSize = 32;
            txtText.Margin = new Thickness(5, 0, 0, 0);
            txtText.SetResourceReference(TextBlock.ForegroundProperty, "HeaderTextColor");
            txtText.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
    
            panelMain.Children.Add(txtText);

            frameBorder.Child = panelMain;

            this.Content = frameBorder;
        }
    }
}
